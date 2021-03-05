using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderProfitabilityGuardService
    {
        private readonly OrderCancellationService orderCancellationService;
        private readonly HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly TwoCryptoCalcAdapter twoCryptoCalc;
        private readonly ILogger logger;
        private readonly ILogger notifier;

        public OrderProfitabilityGuardService(
            OrderCancellationService orderCancellationService,
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter,
            TwoCryptoCalcAdapter twoCryptoCalc, ILoggerFactory loggerFactory)
        {
            this.orderCancellationService = orderCancellationService ?? 
                throw new ArgumentNullException(nameof(orderCancellationService));
            this.hashpowerMarketPrivateAdapter = hashpowerMarketPrivateAdapter ?? 
                throw new ArgumentNullException(nameof(hashpowerMarketPrivateAdapter));
            this.twoCryptoCalc = twoCryptoCalc ?? 
                throw new ArgumentNullException(nameof(twoCryptoCalc));

            if(loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<OrderProfitabilityGuardService>();
            notifier = loggerFactory.CreateNotifier<OrderProfitabilityGuardService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CancelRunningOrderIfPriceGtRewardAsync();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "An error has occurred on OrderProfitabilityGuardService main loop.");
                }

                await Task.Delay(60000);
            }

            logger.LogWarning("OrderProfitabilityGuardService was cancelled!");
        }

        private async Task CancelRunningOrderIfPriceGtRewardAsync()
        {
            var runningOrder = (await hashpowerMarketPrivateAdapter.GetActiveOrdersAsync())
                .SingleOrDefault(o => o.IsRunning);

            if (runningOrder != null)
            {
                logger.LogInformation("Found running order {OrderId}; PriceBtc: {PriceBtc}",
                    runningOrder.Id,
                    runningOrder.PriceBtc);

                var miningAverageRewardBtc = await twoCryptoCalc.GetEthMiningAverageRewardBtcAsync();

                if (runningOrder.PriceBtc > miningAverageRewardBtc * 0.99)
                {
                    notifier.LogInformation("Cancelling order {OrderId} due the price ({PriceBtc}) " +
                        "greater than reward ({MiningAverageRewardBtc})",
                        runningOrder.Id,
                        runningOrder.PriceBtc,
                        miningAverageRewardBtc);

                    await orderCancellationService.CancelOrderAsync(runningOrder.Id);
                }
            }
            else
            {
                logger.LogInformation("There are no running orders.");
            }
        }
    }
}
