﻿using HPBot.Application.Adapters;
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
        private readonly HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly ILogger logger;
        private readonly ILogger notifier;

        public OrderRefillerService(TwoCryptoCalcAdapter twoCryptoCalcAdapter,
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter, ILoggerFactory loggerFactory)
        {
            this.twoCryptoCalcAdapter = twoCryptoCalcAdapter ?? throw new ArgumentNullException(nameof(twoCryptoCalcAdapter));
            this.hashpowerMarketPrivateAdapter = hashpowerMarketPrivateAdapter ?? throw new ArgumentNullException(nameof(hashpowerMarketPrivateAdapter));

            if(loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<OrderRefillerService>();
            notifier = loggerFactory.CreateLogger<OrderRefillerService>();
        }

        public async Task RefillRunningOrderIfApplicableAsync(float refillAmountBtc = 0.001F, 
            float remainAmountBtcThresholdToRefill = 0.00075F)
        {
            var runningOrder = (await hashpowerMarketPrivateAdapter.GetActiveOrdersAsync())
                .SingleOrDefault(o => o.IsRunning);

            if(runningOrder == null)
            {
                logger.LogInformation("There are no running orders");
            }
            else
            {
                var miningAverageReward = await twoCryptoCalcAdapter.GetEthMiningAverageRewardBtcAsync();

                if (runningOrder.PriceBtc <= miningAverageReward)
                {
                    if (runningOrder.RemainAmountBtc <= remainAmountBtcThresholdToRefill)
                    {
                        if (runningOrder.Expires - DateTimeOffset.Now < TimeSpan
                            .FromMinutes(1440F * (refillAmountBtc + runningOrder.RemainAmountBtc) / (runningOrder.PriceBtc * 0.01F)))
                        {
                            logger.LogInformation("Skiping refill order {OrderId} due it is near do expire (CanLiveTill).",
                                runningOrder.Id,
                                runningOrder.Expires);
                        }
                        else
                        {
                            notifier.LogInformation("Refilling order {OrderId} with (RefillAmountBtc) due " +
                                "RemainAmountBtc ({RemainAmountBtc}) <= " +
                                "RemainAmountBtcThresholdToRefill ({RemainAmountBtcThresholdToRefill})",
                                runningOrder.Id,
                                refillAmountBtc,
                                runningOrder.RemainAmountBtc,
                                remainAmountBtcThresholdToRefill);

                            await hashpowerMarketPrivateAdapter.RefillOrder(runningOrder.Id, refillAmountBtc);
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
