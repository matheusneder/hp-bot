using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests.AdaptersIntegrationTests
{
    public class HashpowerMarketPrivateTests
    {
        private readonly HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter =
            new HashpowerMarketPrivateAdapter(Helpers.NiceHashApiPersonedClient);

        [Fact]
        public async Task Hashpower_OrderNotFound_Success()
        {
            Assert.Null(await hashpowerMarketPrivateAdapter.GetOrderByIdAsync(Guid.NewGuid().ToString().ToLower()));
        }

        [Fact]
        public async Task Hashpower_Buy_OrderAmountTooSmall_Error()
        {
            float amountBtc = 0.0001F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.05F;

            var ex = await Assert.ThrowsAsync<CreateOrderException>(async () => await hashpowerMarketPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD"));

            Assert.Equal(CreateOrderException.CreateOrderErrorReason.OrderAmountTooSmall, ex.Reason);
        }

        [Fact]
        public async Task Hashpower_Refill_RefillOrderAmountBelowMinimalOrderAmount_Error()
        {
            float amountBtc = 0.005F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.01F;
            float refillAmount = 0.001F;

            var createdOrder = await hashpowerMarketPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD");

            var ex = await Assert.ThrowsAsync<RefillOrderException>(async () =>
                await hashpowerMarketPrivateAdapter.RefillOrder(createdOrder.Id, refillAmount));

            Assert.Equal(RefillOrderException.RefillOrderExceptionReason.RefillOrderAmountBelowMinimalOrderAmount, 
                ex.Reason);

            await CancelOrderAsync(createdOrder.Id);
        }

        [Fact]
        public async Task Hashpower_Buy_Retrive_List_Refill_Then_Cancel_Success()
        {
            float amountBtc = 0.005F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.01F;
            float refillAmount = 0.005F;

            var createdOrder = await hashpowerMarketPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD");

            Assert.Equal(priceBtc, createdOrder.PriceBtc);
            Assert.True(createdOrder.Expires > DateTimeOffset.Now.AddHours(23));

            // Retriving
            var retrivedOrder = await hashpowerMarketPrivateAdapter.GetOrderByIdAsync(createdOrder.Id);

            Assert.Equal(createdOrder.Id, retrivedOrder.Id);
            Assert.Equal(priceBtc, retrivedOrder.PriceBtc);
            Assert.Equal("ACTIVE", retrivedOrder.Status);
            Assert.True(retrivedOrder.Expires > DateTimeOffset.Now.AddHours(23));

            // Refilling!
            await hashpowerMarketPrivateAdapter.RefillOrder(createdOrder.Id, refillAmount);

            // Getting active orders
            var retrivedOrderFromActiveOrderList =
                (await hashpowerMarketPrivateAdapter.GetActiveOrdersAsync()).Single(o => o.Id == createdOrder.Id);

            Assert.Equal(createdOrder.Id, retrivedOrderFromActiveOrderList.Id);
            Assert.Equal(priceBtc, retrivedOrderFromActiveOrderList.PriceBtc);
            Assert.False(retrivedOrderFromActiveOrderList.IsRunning);
            Assert.True(retrivedOrderFromActiveOrderList.CreatedAt < DateTimeOffset.Now);

            // Check if refill worked
            Assert.Equal(amountBtc + refillAmount, retrivedOrderFromActiveOrderList.AmountBtc);

            // Check order consistence
            Assert.Equal(
                retrivedOrderFromActiveOrderList.SpentWithoutTaxesAmountBtc +
                retrivedOrderFromActiveOrderList.AvailableAmountBtc +
                retrivedOrderFromActiveOrderList.TaxesAmountBtc,
                retrivedOrderFromActiveOrderList.AmountBtc);
            Assert.Equal(
                retrivedOrderFromActiveOrderList.AvailableAmountBtc - 
                retrivedOrderFromActiveOrderList.SpentWithoutTaxesAmountBtc, 
                retrivedOrderFromActiveOrderList.RemainAmountBtc);

            await CancelOrderAsync(createdOrder.Id);
        }

        private async Task CancelOrderAsync(string orderId)
        {
            await hashpowerMarketPrivateAdapter.CancelOrderAsync(orderId);

            // Check if order was cancelled
            Assert.DoesNotContain((await hashpowerMarketPrivateAdapter.GetActiveOrdersAsync()),
                o => o.Id == orderId);
        }
    }
}
