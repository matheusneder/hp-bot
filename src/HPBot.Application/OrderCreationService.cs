using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class OrderCreationService
    {
        private readonly NiceHashApiAdapter niceHash;
        private readonly ILogger logger;

        public OrderCreationService(NiceHashApiAdapter niceHash, ILoggerFactory loggerFactory)
        {
            this.niceHash = niceHash ?? throw new ArgumentNullException(nameof(niceHash));
            logger = loggerFactory?.CreateLogger<OrderCreationService>() ??
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<CreateOrderResult> TryBestOrderAsync(
            PriceProvider priceProvider, 
            float amountBtc, 
            float speedLimitThs, 
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> markets = new Dictionary<string, string>()
            {
                { "USA",  niceHash.Client.Configuration.UsaPoolId },
                { "EU", niceHash.Client.Configuration.EUPoolId }
            };

            // TODO: prevent risk of never create an order by keep minPriceBtc assignment as is
            if (minMarketPriceBtc == float.MaxValue)
            {
                var taskMap = new Dictionary<string, Task<FixedPriceResult>>();

                foreach (var market in markets)
                {
                    taskMap.Add(market.Key, niceHash.GetCurrentFixedPriceAsync(market.Key, speedLimitThs));
                }

                foreach(var marketTaskPair in taskMap)
                {
                    // TODO: handle errors
                    var fixedPriceBtc = (await marketTaskPair.Value).FixedPriceBtc;

                    logger.LogInformation("Price for {Market} market: {MaketPriceBtc}", marketTaskPair.Key, fixedPriceBtc);

                    if (minMarketPriceBtc > fixedPriceBtc)
                    {
                        minMarketPriceBtc = fixedPriceBtc + 0.000001F;
                    }
                }
            }

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    foreach(var market in markets)
                    {
                        var order = await TryOrderAsync(
                            market: market.Key,
                            poolId: market.Value,
                            maxPriceBtc: priceProvider.GoodPriceForANewOrderBtc,
                            amountBtc,
                            speedLimitThs);

                        if (order != null)
                        {
                            logger.LogError("Created order on {Market} market!", market.Key); // TODO: fix log level

                            return order;
                        }
                    }
                }
                catch(CreateOrderException e)
                {
                    logger.LogWarning(e, "Error creating order");
                }
            }

            return null;
        }

        private float minMarketPriceBtc = float.MaxValue;

        /// <exception cref="CreateOrderException" />
        private async Task<CreateOrderResult> TryOrderAsync(
            string market, 
            string poolId, 
            float maxPriceBtc, 
            float amountBtc, 
            float speedLimitThs)
        {
            FixedPriceResult currentPrice;

            try
            {
                currentPrice = await niceHash.GetCurrentFixedPriceAsync(market, speedLimitThs);
            }
            catch(GetCurrentFixedPriceException e) when (e.Reason == GetCurrentFixedPriceException.GetCurrentFixedPriceErrorReason.FixedOrderPriceQuerySpeedLimitTooBig)
            {
                logger.LogWarning(e, $"Could not get current fixed price on {market} market. " +
                    $"There are no more hashpower available for speed limit of {speedLimitThs} TH/s at this moment.");

                return null;
            }
            catch (NiceHashApiTechnicalIssueException e)
            {
                logger.LogWarning(e, $"Could not get current fixed price on {market} market. " +
                    $"Request failed, wating for 5 seconds to resume...");

                await Task.Delay(5000);

                return null;
            }

            if(currentPrice.FixedPriceBtc < minMarketPriceBtc)
            {
                minMarketPriceBtc = currentPrice.FixedPriceBtc;

                logger.LogInformation(
                    "{Time} - MinPriceBtc: {MinPriceBtc}; MinPriceMarket: {MinPriceMarket}",
                    DateTimeOffset.Now,
                    minMarketPriceBtc,
                    market);
            }

            logger.LogDebug(
                "Price query result - Market: {Market}; CurrentPriceBtc: {CurrentPriceBtc};",
                market, 
                currentPrice.FixedPriceBtc);

            if(currentPrice.FixedPriceBtc <= maxPriceBtc)
            {
                logger.LogInformation(
                    "Price found! Market: {Market}; CurrentPriceBtc: {CurrentPriceBtc}",
                    market,
                    currentPrice.FixedPriceBtc);

                return await niceHash.CreateOrderAsync(market, amountBtc,
                    currentPrice.FixedPriceBtc, speedLimitThs, poolId, "FIXED");
            }

            return null;
        }

        
    }
}
