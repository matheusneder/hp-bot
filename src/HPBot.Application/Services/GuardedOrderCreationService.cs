using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Services
{
    public class GuardedOrderCreationService
    {
        private readonly IOrderCreationService orderCreationService;
        private readonly IOrderCreationBlockerService orderCreationBlockerService;
        private readonly IHashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly ILogger logger;
        private const int MaxQueryActiveOrdersErrors = 5;

        public GuardedOrderCreationService(IOrderCreationService orderCreationService, 
            IOrderCreationBlockerService orderCreationBlockerService,
            IHashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter,
            ILoggerFactory loggerFactory)
        {
            this.orderCreationService = orderCreationService ?? 
                throw new ArgumentNullException(nameof(orderCreationService));
            this.orderCreationBlockerService = orderCreationBlockerService ?? 
                throw new ArgumentNullException(nameof(orderCreationBlockerService));
            this.hashpowerMarketPrivateAdapter = hashpowerMarketPrivateAdapter ?? 
                throw new ArgumentNullException(nameof(hashpowerMarketPrivateAdapter));
            logger = loggerFactory?.CreateLogger<GuardedOrderCreationService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<CreateOrderResult> TryOrderAsync(string market, string poolId, float maxPriceBtc, float amountBtc, float speedLimitThs, ICollection<Order> activeOrders)
        {
            if (await orderCreationBlockerService.ShouldCreateANewOrderAsync())
            {
                try
                {
                    var order = await orderCreationService.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs);
                    activeOrders.Add(order);

                    return order;
                }
                catch(NiceHashApiTechnicalIssueException e)
                {
                    // At this point, order should be created or not!
                    // Will query nicehash in order to discover if a new order was created.

                    logger.LogWarning(e, $"NiceHashApiTechnicalIssueException while Trying order: {e.Message}");

                    await Task.Delay(1000); // Give chances to niceshash take a breath

                    IEnumerable<ListOrderResultItem> activeOrdersOnNicehash = await QueryActiveOrdersWithRetryOnFailAsync();

                    var unknowOrders = activeOrdersOnNicehash.Where(n => !activeOrders.Any(a => a.Id == n.Id));

                    if (unknowOrders.Count() == 1)
                    {
                        // Should be an order created by above orderCreationService.TryOrderAsync call
                        var inferredOrderCandidate = unknowOrders.Single();

                        if (inferredOrderCandidate.CreatedAt < DateTimeOffset.Now.AddSeconds(5) &&
                            inferredOrderCandidate.CreatedAt > DateTimeOffset.Now.AddSeconds(-5))
                        {
                            // ops, an order was very recently created

                            // TODO: include market (and maybe other fields) for check purpose
                            if (inferredOrderCandidate.AmountBtc == amountBtc && inferredOrderCandidate.PriceBtc <= maxPriceBtc)
                            {
                                // Assume that the order was successfully created!

                                var inferredOrder = new CreateOrderResult()
                                {
                                    Id = inferredOrderCandidate.Id,
                                    MarketFactor = inferredOrderCandidate.MarketFactor,
                                    PriceBtc = inferredOrderCandidate.PriceBtc,
                                    Expires = inferredOrderCandidate.Expires
                                };

                                logger.LogWarning(
                                    "Despite NiceHashApiTechnicalIssueException has occured, " +
                                    "it's being assumed the order was successfully created. InferredOrderId: {InferredOrderId}",
                                    inferredOrder.Id);

                                activeOrders.Add(inferredOrder);

                                return inferredOrder;
                            }
                        }
                    }

                    if (unknowOrders.Any())
                    {
                        throw new InvalidOperationException(
                            $"An unexpected state was encountered: " +
                            $"There are ({unknowOrders.Count()}) unkown active orders on nicehash. " +
                            $"Unknow order ids: {string.Join(';', unknowOrders.Select(u => u.Id))}");
                    }

                    logger.LogWarning("After queried NiceHash active order lists, assumed the no new order was created, so resuming regular flow.");

                    return null;
                }
            }
            else
            {
                throw new OrderCreationException(OrderCreationException.CreateOrderErrorReason.OrderCreationBlocked);
            }
        }

        private async Task<IEnumerable<ListOrderResultItem>> QueryActiveOrdersWithRetryOnFailAsync()
        {
            int errorCount = 0;

            for(; ; )
            {
                try
                {
                    return await hashpowerMarketPrivateAdapter
                        .GetActiveOrdersAsync();
                }
                catch (NiceHashApiTechnicalIssueException e)
                {
                    if(errorCount >= MaxQueryActiveOrdersErrors)
                    {
                        logger.LogWarning(e, $"Error while trying to query active orders (this was the last attempt, the original exception is being thrown): {e.Message}");

                        throw;
                    }

                    logger.LogWarning(e, $"Error while trying to query active orders (retrying): {e.Message}");
                    await Task.Delay(1000 + errorCount * 500);
                    errorCount++;
                }
            }
        }
    }
}
