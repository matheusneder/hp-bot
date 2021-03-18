using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class PriceProvider
    {
        private readonly ILogger logger;
        private float currentAverageRewardBtc = float.MinValue;
        public float MinProfabilityPercent { get; set; } = 0.06F;
        // - 0.00002000F order no refundable tax

        public PriceProvider(TwoCryptoCalcAdapter twoCryptoCalc, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<PriceProvider>() ??
                throw new ArgumentNullException(nameof(loggerFactory));

            Task.Factory.StartNew(async () =>
            {
                int errorCount = 0;

                for(; ; )
                {
                    try
                    {
                        var newAverageRewardBtc =
                            await twoCryptoCalc.GetEthMiningAverageRewardBtcAsync();
                        
                        errorCount = 0;

                        if (newAverageRewardBtc != currentAverageRewardBtc)
                        {
                            currentAverageRewardBtc = newAverageRewardBtc;
                            
                            logger.LogInformation(
                                "TwoCryptoCalc average reward changed: {CurrentProfabilityBtc}, MaxPriceBtcBasedOnReward: {MaxPriceBtcBasedOnReward}",
                                currentAverageRewardBtc, MaxPriceBtcBasedOnReward);
                        }

                        await Task.Delay(30000);
                    }
                    catch(Exception e)
                    {
                        errorCount++;
                        logger.LogWarning(e, "Error retriving 2CryptoCalc mining average reward {ErrorCount}.", errorCount);
                        
                        if(errorCount > 30 && currentAverageRewardBtc > 0)
                        {
                            currentAverageRewardBtc = float.MinValue;

                            logger.LogWarning("Error count greater than 30, reseting currentAverageRewardBtc to float.MinValue");
                        }
                        
                        await Task.Delay(5000);
                    }
                    
                    
                }
            }, TaskCreationOptions.LongRunning);
        }

        public Order RunningOrder { get; set; }

        private float GetABetterPriceThanTheRunningOrderPrice()
        {
            if(RunningOrder == null)
            {
                return float.MaxValue;
            }

            var timeTillExpire = RunningOrder.Expires - DateTimeOffset.Now;

            if (timeTillExpire < TimeSpan.FromHours(2))
            {
                return float.MaxValue;
            }

            if (timeTillExpire < TimeSpan.FromHours(3))
            {
                return RunningOrder.PriceBtc * 1.5F;
            }

            if (timeTillExpire < TimeSpan.FromHours(4))
            {
                return RunningOrder.PriceBtc * 1.2F;
            }

            if (timeTillExpire < TimeSpan.FromHours(6))
            {
                return RunningOrder.PriceBtc * 1.1F;
            }

            if (timeTillExpire < TimeSpan.FromHours(7))
            {
                return RunningOrder.PriceBtc * 1.07F;
            }

            if (timeTillExpire < TimeSpan.FromHours(8))
            {
                return RunningOrder.PriceBtc * 1.03F;
            }

            if (timeTillExpire < TimeSpan.FromHours(12))
            {
                return RunningOrder.PriceBtc * 1.01F;
            }

            if (timeTillExpire < TimeSpan.FromHours(16))
            {
                return RunningOrder.PriceBtc + 0.000001F;
            }

            if (timeTillExpire < TimeSpan.FromHours(19))
            {
                return RunningOrder.PriceBtc * 0.99F;
            }

            if (timeTillExpire < TimeSpan.FromHours(21))
            {
                return RunningOrder.PriceBtc * 0.983F;
            }

            return RunningOrder.PriceBtc * 0.975F;
        }

        public float MaxPriceBtcBasedOnReward =>currentAverageRewardBtc * (1.0F - MinProfabilityPercent);

        public float GoodPriceForANewOrderBtc =>
            Math.Min(MaxPriceBtcBasedOnReward, GetABetterPriceThanTheRunningOrderPrice());
    }
}