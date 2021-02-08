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
        private readonly NiceHashApiAdapter niceHashApi;
        private readonly TwoCryptoCalcAdapter twoCryptoCalc;
        private readonly ILogger logger;

        public OrderProfitabilityGuardService(
            OrderCancellationService orderCancellationService,
            NiceHashApiAdapter niceHashApi,
            TwoCryptoCalcAdapter twoCryptoCalc, ILoggerFactory loggerFactory)
        {
            this.orderCancellationService = orderCancellationService ?? 
                throw new ArgumentNullException(nameof(orderCancellationService));
            this.niceHashApi = niceHashApi ?? 
                throw new ArgumentNullException(nameof(niceHashApi));
            this.twoCryptoCalc = twoCryptoCalc ?? 
                throw new ArgumentNullException(nameof(twoCryptoCalc));
            logger = loggerFactory?.CreateLogger<OrderProfitabilityGuardService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
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
            var runningOrder = (await niceHashApi.GetActiveOrders())
                .SingleOrDefault(o => o.IsRunning);

            if (runningOrder != null)
            {
                logger.LogInformation("Found running order {OrderId}; PriceBtc: {PriceBtc}",
                    runningOrder.Id,
                    runningOrder.PriceBtc);

                var miningAverageRewardBtc = await twoCryptoCalc.GetEthMiningAverageRewardBtcAsync();

                if (runningOrder.PriceBtc > miningAverageRewardBtc * 0.99)
                {
                    logger.LogError("Cancelling order {OrderId} due the price ({PriceBtc}) " + // TODO: fix log level
                        "greater than reward ({MiningAverageRewardBtc})",
                        runningOrder.Id,
                        runningOrder.PriceBtc,
                        miningAverageRewardBtc);

                    await orderCancellationService.CancelOrderAsync(runningOrder.Id);
                }
            }
            else
            {
                logger.LogInformation("There is no running orders.");
            }
        }
    }
}
