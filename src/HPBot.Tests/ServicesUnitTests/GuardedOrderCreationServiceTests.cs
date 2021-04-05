using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using HPBot.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests.ServicesUnitTests
{
    public class GuardedOrderCreationServiceTests
    {
        private readonly Mock<IHashpowerMarketPrivateAdapter> hashpowerMarketPrivateAdapterMock =
            new Mock<IHashpowerMarketPrivateAdapter>();
        private readonly Mock<IOrderCreationBlockerService> orderCreationBlockerServiceMock =
            new Mock<IOrderCreationBlockerService>();
        private readonly Mock<IOrderCreationService> orderCreationServiceMock =
            new Mock<IOrderCreationService>();

        private GuardedOrderCreationService GuardedOrderCreationService =>
            new GuardedOrderCreationService(
                orderCreationServiceMock.Object,
                orderCreationBlockerServiceMock.Object,
                hashpowerMarketPrivateAdapterMock.Object,
                new LoggerFactory());

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

            orderCreationServiceMock.Setup(m =>
                        m.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs))
                    .ReturnsAsync(new CreateOrderResult()
                    {
                        Id = orderId,
                        Expires = DateTimeOffset.Now.AddHours(24),
                        PriceBtc = priceBtc,
                        MarketFactor = marketFactor
                    });

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(true);

            var activeOrders = new List<Order>();

            var createOrderResult = await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders);

            Assert.Collection(activeOrders, i => Assert.Equal(orderId, i.Id));
            Assert.Equal(orderId, createOrderResult.Id);
            Assert.Equal(priceBtc, createOrderResult.PriceBtc);
            Assert.InRange(createOrderResult.Expires,
                DateTimeOffset.Now.AddHours(23),
                DateTimeOffset.Now.AddHours(25));
            Assert.Equal(marketFactor, createOrderResult.MarketFactor);
        }

        [Fact]
        public async Task TryOrder_NiceHashApiTechnicalIssueException_But_Created_Success()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;
            float priceBtc = 2.99F;
            string orderId = Guid.NewGuid().ToString().ToLower();
            float marketFactor = 1000000000;

            // GetActiveOrdersAsync will return the list with an order simulating the order was created
            hashpowerMarketPrivateAdapterMock.Setup(m => m.GetActiveOrdersAsync())
                .ReturnsAsync(new ListOrderResultItem[] 
                {
                    new ListOrderResultItem()
                    {
                        AmountBtc = amountBtc,
                        PriceBtc = priceBtc,
                        CreatedAt = DateTimeOffset.Now,
                        Id = orderId,
                        MarketFactor = 1000000000,
                        Expires = DateTimeOffset.Now.AddDays(1)
                    }
                });
           
            hashpowerMarketPrivateAdapterMock.Setup(m =>
                        m.CreateOrderAsync(market, amountBtc, priceBtc, speedLimitThs, poolId, "FIXED"))
                    .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("dummy")));

            orderCreationServiceMock.Setup(m =>
                        m.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs))
                    .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("dummy")));

            var activeOrders = new List<Order>();

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(true);

            var createOrderResult = await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders);

            Assert.Collection(activeOrders, i => Assert.Equal(orderId, i.Id));
            Assert.Equal(orderId, createOrderResult.Id);
            Assert.Equal(priceBtc, createOrderResult.PriceBtc);
            Assert.InRange(createOrderResult.Expires,
                DateTimeOffset.Now.AddHours(23),
                DateTimeOffset.Now.AddHours(25));
            Assert.Equal(marketFactor, createOrderResult.MarketFactor);
        }


        [Fact]
        public async Task TryOrder_NiceHashApiTechnicalIssueException_OrderNotCreated_Success()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            hashpowerMarketPrivateAdapterMock.Setup(m => m.GetActiveOrdersAsync())
                .ReturnsAsync(new ListOrderResultItem[] { });

            orderCreationServiceMock.Setup(m =>
                        m.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs))
                    .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("dummy")));

            var activeOrders = new List<Order>();

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(true);

            var createOrderResult = await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders);
            
            Assert.Null(createOrderResult);
            Assert.Empty(activeOrders);
        }

        [Fact]
        public async Task TryOrder_NiceHashApiTechnicalIssueException_UnknowOrders_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;
            string zumbieOrderId = Guid.NewGuid().ToString().ToLower();

            hashpowerMarketPrivateAdapterMock.Setup(m => m.GetActiveOrdersAsync())
                .ReturnsAsync(new ListOrderResultItem[]
                {
                    new ListOrderResultItem()
                    {
                        CreatedAt = DateTimeOffset.Now.AddMinutes(-15),
                        Id = zumbieOrderId,
                        Expires = DateTimeOffset.Now.AddMinutes(-15).AddDays(1),
                        MarketFactor = 1000000000,
                        PriceBtc = 3F
                    }
                });

            orderCreationServiceMock.Setup(m =>
                        m.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs))
                    .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("dummy")));

            var activeOrders = new List<Order>();

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders));

            Assert.Contains(zumbieOrderId, ex.Message);
        }

        [Fact]
        public async Task TryOrder_OrderCreationBlocked_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(false);

            var activeOrders = new List<Order>();

            var ex = await Assert.ThrowsAsync<OrderCreationException>(async () => await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders));

            Assert.Equal(OrderCreationException.CreateOrderErrorReason.OrderCreationBlocked, ex.Reason);
        }

        [Fact]
        public async Task TryOrder_And_GetActiveOrders_NiceHashApiTechnicalIssueException_Error()
        {
            string market = "USA";
            string poolId = "xpto";
            float maxPriceBtc = 3F;
            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            hashpowerMarketPrivateAdapterMock.Setup(m => m.GetActiveOrdersAsync())
                .ThrowsAsync(new NiceHashApiReadResponseException(new Exception("dummy")));

            orderCreationServiceMock.Setup(m =>
                        m.TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs))
                    .ThrowsAsync(new NiceHashApiSendRequestException(new Exception("dummy")));

            var activeOrders = new List<Order>();

            orderCreationBlockerServiceMock.Setup(m => m.ShouldCreateANewOrderAsync())
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<NiceHashApiReadResponseException>(async () => await GuardedOrderCreationService
                .TryOrderAsync(market, poolId, maxPriceBtc, amountBtc, speedLimitThs, activeOrders));
        }
    }
}
