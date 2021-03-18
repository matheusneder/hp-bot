using HPBot.Application.Adapters;
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
        private readonly HashpowerMarketPublicAdapter hashpowerMarketPublicAdapter;
        private readonly HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly ILogger logger;
        private readonly ILogger notifier;

        public OrderCreationService(HashpowerMarketPublicAdapter hashpowerMarketPublicAdapter, 
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter, ILoggerFactory loggerFactory)
        {
            this.hashpowerMarketPublicAdapter = hashpowerMarketPublicAdapter ?? 
                throw new ArgumentNullException(nameof(hashpowerMarketPublicAdapter));

            this.hashpowerMarketPrivateAdapter = hashpowerMarketPrivateAdapter ?? 
                throw new ArgumentNullException(nameof(hashpowerMarketPrivateAdapter));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<OrderCreationService>();
            notifier = loggerFactory.CreateNotifier<OrderCreationService>();
        }

        public async Task<CreateOrderResult> TryBestOrderAsync(
            PriceProvider priceProvider, 
            float amountBtc, 
            float speedLimitThs, 
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> markets = new Dictionary<string, string>()
            {
                { "USA",  hashpowerMarketPublicAdapter.Client.Configuration.UsaPoolId },
                { "EU", hashpowerMarketPublicAdapter.Client.Configuration.EUPoolId }
            };

            // TODO: prevent risk of never create an order by keep minPriceBtc assignment as is
            if (minMarketPriceBtc == float.MaxValue)
            {
                var taskMap = new Dictionary<string, Task<FixedPriceResult>>();

                foreach (var market in markets)
                {
                    taskMap.Add(market.Key, hashpowerMarketPublicAdapter.GetCurrentFixedPriceAsync(market.Key, speedLimitThs));
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
                            notifier.LogInformation("Created order on {Market} market!", market.Key);

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
                currentPrice = await hashpowerMarketPublicAdapter.GetCurrentFixedPriceAsync(market, speedLimitThs);
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

                return await hashpowerMarketPrivateAdapter.CreateOrderAsync(market, amountBtc,
                    currentPrice.FixedPriceBtc, speedLimitThs, poolId, "FIXED");
            }

            return null;
        }

        
    }
}
