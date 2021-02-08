using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderCancellationService
    {
        private readonly NiceHashApiAdapter niceHashApi;
        private readonly ILogger logger;

        public OrderCancellationService(NiceHashApiAdapter niceHashApi, ILoggerFactory loggerFactory)
        {
            this.niceHashApi = niceHashApi ?? throw new ArgumentNullException(nameof(niceHashApi));
            logger = loggerFactory?.CreateLogger<OrderCancellationService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }


        public async Task CancelOrderAsync(string orderId)
        {
            bool orderCancelled = false;

            while (!orderCancelled) // PANIC LOOP
            {
                try
                {
                    await niceHashApi.CancelOrderAsync(orderId);
                    orderCancelled = true;
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Error while trying to cancel order {OrderId}",
                        orderId);

                    var freshOrderDetails = await niceHashApi.GetOrderById(orderId);

                    if (freshOrderDetails == null)
                    {
                        logger.LogWarning("Order {OrderId} not found! " +
                            "This is an unexpected state since the order was previously created.",
                            orderId);
                    }
                    else
                    {
                        switch (freshOrderDetails.Status)
                        {
                            case "PENDING":
                            case "ACTIVE":
                                break;
                            default:
                                logger.LogInformation("Despite error, considering order {OrderId} as cancelled " +
                                    " due its retrived status {Status}", orderId, freshOrderDetails.Status);
                                orderCancelled = true;
                                break;
                        }
                    }

                    await Task.Delay(1000);
                }
            }
        }
    }
}
