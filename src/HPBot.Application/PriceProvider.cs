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
                        
                        if(errorCount > 5 && currentAverageRewardBtc > 0)
                        {
                            currentAverageRewardBtc = float.MinValue;

                            logger.LogWarning("Error count greater than 5, reseting currentAverageRewardBtc to float.MinValue");
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

            var timeTillExpire = RunningOrder.CanLiveTill - DateTimeOffset.Now;

            if (timeTillExpire < TimeSpan.FromHours(1))
            {
                return RunningOrder.PriceBtc * 1.03F;
            }

            if (timeTillExpire < TimeSpan.FromHours(2))
            {
                return RunningOrder.PriceBtc * 1.015F;
            }

            if (timeTillExpire < TimeSpan.FromHours(12))
            {
                return RunningOrder.PriceBtc + 0.00001F;
            }

            if (timeTillExpire < TimeSpan.FromHours(18))
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