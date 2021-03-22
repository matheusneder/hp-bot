using HPBot.Application.Adapters;
using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderCreationFlowService
    {
        private readonly DeprecatedOrderCreationService orderCreationService;
        private readonly OrderCancellationService orderCancellationService;
        private readonly HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly TwoCryptoCalcAdapter twoCryptoCalc;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        public OrderCreationFlowService(
            DeprecatedOrderCreationService orderCreationService, 
            OrderCancellationService orderCancellationService,
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter, 
            TwoCryptoCalcAdapter twoCryptoCalc, 
            ILoggerFactory loggerFactory)
        {
            this.orderCreationService = orderCreationService ?? 
                throw new ArgumentNullException(nameof(orderCreationService));
            this.orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));
            this.hashpowerMarketPrivateAdapter = hashpowerMarketPrivateAdapter ?? 
                throw new ArgumentNullException(nameof(hashpowerMarketPrivateAdapter));
            this.twoCryptoCalc = twoCryptoCalc ?? throw new ArgumentNullException(nameof(twoCryptoCalc));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            logger = loggerFactory.CreateLogger<OrderCreationFlowService>();
        }

        public async Task StartAsync(float speedLimitThs = 0.01F, float amountBtc = 0.001F)
        {
            PriceProvider priceProvider = new PriceProvider(twoCryptoCalc, loggerFactory);

            priceProvider.RunningOrder = (await hashpowerMarketPrivateAdapter.GetActiveOrdersAsync())
                .SingleOrDefault(o => o.IsRunning);

            if (priceProvider.RunningOrder == null)
            {
                logger.LogInformation("There was no order running, will create one if find good market conditions...");

                priceProvider.RunningOrder = await orderCreationService.TryBestOrderAsync(
                    priceProvider,
                    amountBtc,
                    speedLimitThs);
            }
            else
            {
                logger.LogInformation(
                    "Found a running order {OrderId}", priceProvider.RunningOrder.Id);
            }

            for (; ; )
            {
                var newOrder = await orderCreationService.TryBestOrderAsync(
                                priceProvider,
                                amountBtc,
                                speedLimitThs);

                if (newOrder != null)
                {
                    if (priceProvider.RunningOrder != null)
                    {
                        await orderCancellationService.CancelOrderAsync(priceProvider.RunningOrder.Id);
                    }

                    priceProvider.RunningOrder = newOrder;
                }
            }

        }
    }
}
