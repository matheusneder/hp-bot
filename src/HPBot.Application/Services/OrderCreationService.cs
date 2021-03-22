using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Services
{
    public class OrderCreationService
    {
        private readonly IHashpowerMarketPublicAdapter hashpowerMarketPublicAdapter;
        private readonly IHashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter;
        private readonly ILogger logger;

        public OrderCreationService(IHashpowerMarketPublicAdapter hashpowerMarketPublicAdapter,
            IHashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter, ILoggerFactory loggerFactory)
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
        }

        /// <exception cref="CreateOrderException" />
        public async Task<CreateOrderResult> TryOrderAsync(
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
            catch (GetCurrentFixedPriceException e) when (e.Reason == GetCurrentFixedPriceException.GetCurrentFixedPriceErrorReason.FixedOrderPriceQuerySpeedLimitTooBig)
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

            logger.LogDebug(
                "Price query result - Market: {Market}; CurrentPriceBtc: {CurrentPriceBtc};",
                market,
                currentPrice.FixedPriceBtc);

            if (currentPrice.FixedPriceBtc <= maxPriceBtc)
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
