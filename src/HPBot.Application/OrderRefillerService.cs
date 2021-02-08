using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderRefillerService
    {
        private readonly TwoCryptoCalcAdapter twoCryptoCalcAdapter;
        private readonly NiceHashApiAdapter niceHashApi;
        private readonly ILogger logger;

        public OrderRefillerService(TwoCryptoCalcAdapter twoCryptoCalcAdapter,
            NiceHashApiAdapter niceHashApi, ILoggerFactory loggerFactory)
        {
            this.twoCryptoCalcAdapter = twoCryptoCalcAdapter ?? throw new ArgumentNullException(nameof(twoCryptoCalcAdapter));
            this.niceHashApi = niceHashApi ?? throw new ArgumentNullException(nameof(niceHashApi));
            logger = loggerFactory?.CreateLogger<OrderRefillerService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task RefillRunningOrderIfApplicableAsync(float refillAmountBtc = 0.001F, 
            float remainAmountBtcThresholdToRefill = 0.0005F)
        {
            var runningOrder = (await niceHashApi.GetActiveOrders())
                .SingleOrDefault(o => o.IsRunning);

            if(runningOrder == null)
            {
                logger.LogInformation("There is no running orders");
            }
            else
            {
                var miningAverageReward = await twoCryptoCalcAdapter.GetEthMiningAverageRewardBtcAsync();

                if (runningOrder.PriceBtc <= miningAverageReward)
                {
                    if (runningOrder.RemainAmountBtc <= remainAmountBtcThresholdToRefill)
                    {
                        if (runningOrder.CanLiveTill - DateTimeOffset.Now < TimeSpan.FromMinutes(40))
                        {
                            logger.LogInformation("Skiping refill order {OrderId} due it is near do expire (CanLiveTill).",
                                runningOrder.Id,
                                runningOrder.CanLiveTill);
                        }
                        else
                        {
                            logger.LogError("Refilling order {OrderId} with (RefillAmountBtc) due " + // TODO: fix log level
                                "RemainAmountBtc ({RemainAmountBtc}) <= " +
                                "RemainAmountBtcThresholdToRefill ({RemainAmountBtcThresholdToRefill})",
                                runningOrder.Id,
                                refillAmountBtc,
                                runningOrder.RemainAmountBtc,
                                remainAmountBtcThresholdToRefill);

                            await niceHashApi.RefillOrder(runningOrder.Id, refillAmountBtc);
                        }
                    }
                    else
                    {
                        logger.LogInformation("Skiping refill order {OrderId} due " +
                            "RemainAmountBtc ({RemainAmountBtc}) > " +
                            "RemainAmountBtcThresholdToRefill ({RemainAmountBtcThresholdToRefill})",
                            runningOrder.Id,
                            runningOrder.RemainAmountBtc,
                            remainAmountBtcThresholdToRefill);
                    }
                }
                else
                {
                    logger.LogWarning("Skiping refill order {OrderId} due " +
                        "PriceBtc ({PriceBtc}) > " +
                        "MiningAverageReward ({MiningAverageReward })",
                        runningOrder.Id,
                        runningOrder.PriceBtc,
                        miningAverageReward);
                }
            }
        }
    }
}
