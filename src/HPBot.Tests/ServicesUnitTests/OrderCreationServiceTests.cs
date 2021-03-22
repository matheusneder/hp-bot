using HPBot.Application.Services;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using HPBot.Application.Models;
using HPBot.Application.Exceptions;
using System.Diagnostics;

namespace HPBot.Tests.ServicesUnitTests
{
    public class OrderCreationServiceTests
    {
        private readonly Mock<IHashpowerMarketPublicAdapter> hashpowerMarketPublicAdapterMock =
            new Mock<IHashpowerMarketPublicAdapter>();
        private readonly Mock<IHashpowerMarketPrivateAdapter> hashpowerMarketPrivateAdapterMock = 
            new Mock<IHashpowerMarketPrivateAdapter>();
        private OrderCreationService OrderCreationService => new OrderCreationService(hashpowerMarketPublicAdapterMock.Object,
                hashpowerMarketPrivateAdapterMock.Object, new LoggerFactory());

        [Fact]
        public async Task TryOrder_Success()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;
            float priceBtc = 2.99F;
            string orderId = Guid.NewGuid().ToString().ToLower();
            float marketFactor = 1000000000;

            hashpowerMarketPublicAdapterMock.Setup(m => 
                    m.GetCurrentFixedPriceAsync(market, speedLimitThs))
                .ReturnsAsync(new FixedPriceResult()
                {
                    FixedPriceBtc = priceBtc,
                    MaxSpeedThs = 1F
                });

            hashpowerMarketPrivateAdapterMock.Setup(m =>
                        m.CreateOrderAsync(market, amountBtc, priceBtc, speedLimitThs, poolId, "FIXED"))
                    .ReturnsAsync(new CreateOrderResult()
                    {
                        Id = orderId,
                        Expires = DateTimeOffset.Now.AddHours(24),
                        PriceBtc = priceBtc,
                        MarketFactor = marketFactor
                    });

            var createOrderResult = await OrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs);

            Assert.Equal(orderId, createOrderResult.Id);
            Assert.Equal(priceBtc, createOrderResult.PriceBtc);
            Assert.InRange(createOrderResult.Expires,
                DateTimeOffset.Now.AddHours(23),
                DateTimeOffset.Now.AddHours(25));
            Assert.Equal(marketFactor, createOrderResult.MarketFactor);
        }

        [Fact]
        public async Task TryOrder_CurrentPriceGtMaxPrice_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;
            float priceBtc = 3.01F;
            string orderId = Guid.NewGuid().ToString().ToLower();
            float marketFactor = 1000000000;

            hashpowerMarketPublicAdapterMock.Setup(m =>
                    m.GetCurrentFixedPriceAsync(market, speedLimitThs))
                .ReturnsAsync(new FixedPriceResult()
                {
                    FixedPriceBtc = priceBtc,
                    MaxSpeedThs = 1F
                });

            hashpowerMarketPrivateAdapterMock.Setup(m =>
                        m.CreateOrderAsync(market, amountBtc, priceBtc, speedLimitThs, poolId, "FIXED"))
                    .ReturnsAsync(new CreateOrderResult()
                    {
                        Id = orderId,
                        Expires = DateTimeOffset.Now.AddHours(24),
                        PriceBtc = priceBtc,
                        MarketFactor = marketFactor
                    });

            var createOrderResult = await OrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs);

            Assert.Null(createOrderResult);
        }

        [Fact]
        public async Task TryOrder_GetCurrentFixedPriceAsync_NiceHashApiTechnicalIssueException_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            hashpowerMarketPublicAdapterMock.Setup(m =>
                    m.GetCurrentFixedPriceAsync(market, speedLimitThs))
                .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("Dummy")));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var createOrderResult = await OrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs);

            stopwatch.Stop();
            Assert.InRange(stopwatch.ElapsedMilliseconds, 5000, long.MaxValue);
            Assert.Null(createOrderResult);
        }

        [Fact]
        public async Task TryOrder_GetCurrentFixedPriceAsync_FixedOrderPriceQuerySpeedLimitTooBig_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            hashpowerMarketPublicAdapterMock.Setup(m =>
                    m.GetCurrentFixedPriceAsync(market, speedLimitThs))
                .ThrowsAsync(new GetCurrentFixedPriceException(GetCurrentFixedPriceException
                    .GetCurrentFixedPriceErrorReason.FixedOrderPriceQuerySpeedLimitTooBig));

            var createOrderResult = await OrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs);

            Assert.Null(createOrderResult);
        }
    }
}
