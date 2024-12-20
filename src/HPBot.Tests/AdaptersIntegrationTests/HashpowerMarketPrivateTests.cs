﻿using HPBot.Application.Adapters;
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
        private HashpowerMarketPrivateAdapter HPPrivateAdapter =>
            new HashpowerMarketPrivateAdapter(Helpers.NiceHashApiPersonedClient);

        [Fact]
        public async Task Hashpower_OrderNotFound_Success()
        {
            Assert.Null(await HPPrivateAdapter.GetOrderByIdAsync(Guid.NewGuid().ToString().ToLower()));
        }

        [Fact]
        public async Task Hashpower_Buy_OrderAmountTooSmall_Error()
        {
            float amountBtc = 0.0001F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.05F;

            var ex = await Assert.ThrowsAsync<OrderCreationException>(async () => await HPPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD"));

            Assert.Equal(OrderCreationException.CreateOrderErrorReason.OrderAmountTooSmall, ex.Reason);
        }

        [Fact]
        public async Task Hashpower_Refill_RefillOrderAmountBelowMinimalOrderAmount_Error()
        {
            float amountBtc = 0.005F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.01F;
            float refillAmount = 0.0001F;

            var createdOrder = await HPPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD");

            var ex = await Assert.ThrowsAsync<OrderRefillException>(async () =>
                await HPPrivateAdapter.RefillOrder(createdOrder.Id, refillAmount));

            Assert.Equal(OrderRefillException.RefillOrderErrorReason.RefillOrderAmountBelowMinimalOrderAmount, 
                ex.Reason);
            Assert.Matches(Helpers.NiceHashIdPattern, ex.OrderId);
            Assert.Equal(refillAmount, ex.AmountBtc);

            await CancelOrderAsync(createdOrder.Id);
        }

        [Fact]
        public async Task Hashpower_Buy_Retrive_List_Refill_Then_Cancel_Success()
        {
            float amountBtc = 0.005F;
            float priceBtc = 1.0F;
            float speedLimitThs = 0.01F;
            float refillAmount = 0.005F;
            float expectedMarketFactor = 1000000000000F;

            var createdOrder = await HPPrivateAdapter
                .CreateOrderAsync("USA", amountBtc, priceBtc, speedLimitThs, Helpers.Configuration.UsaPoolId, "STANDARD");

            Assert.Equal(priceBtc, createdOrder.PriceBtc);
            Assert.InRange(createdOrder.Expires,
                // Fixed order duration
                DateTimeOffset.Now.AddHours(23),
                // Standard order duration
                DateTimeOffset.Now.AddDays(10));
            Assert.Equal(expectedMarketFactor, createdOrder.MarketFactor);

            // Retriving
            var retrivedOrder = await HPPrivateAdapter.GetOrderByIdAsync(createdOrder.Id);

            Assert.Equal(createdOrder.Id, retrivedOrder.Id);
            Assert.Equal(priceBtc, retrivedOrder.PriceBtc);
            Assert.Equal("ACTIVE", retrivedOrder.Status);
            Assert.InRange(retrivedOrder.Expires, 
                // Fixed order duration
                DateTimeOffset.Now.AddHours(23),
                // Standard order duration
                DateTimeOffset.Now.AddDays(10));
            Assert.Equal(expectedMarketFactor, retrivedOrder.MarketFactor);

            // Refilling!
            await HPPrivateAdapter.RefillOrder(createdOrder.Id, refillAmount);

            // Getting active orders
            var retrivedOrderFromActiveOrderList =
                (await HPPrivateAdapter.GetActiveOrdersAsync()).Single(o => o.Id == createdOrder.Id);

            Assert.Equal(createdOrder.Id, retrivedOrderFromActiveOrderList.Id);
            Assert.Equal(priceBtc, retrivedOrderFromActiveOrderList.PriceBtc);
            Assert.False(retrivedOrderFromActiveOrderList.IsRunning);
            Assert.InRange(retrivedOrderFromActiveOrderList.CreatedAt, 
                DateTimeOffset.Now.AddSeconds(-10), 
                DateTimeOffset.Now.AddSeconds(10));
            Assert.InRange(retrivedOrderFromActiveOrderList.EstimateDurationInSeconds, 0, int.MaxValue);
            Assert.Equal(expectedMarketFactor, retrivedOrderFromActiveOrderList.MarketFactor);

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
            await HPPrivateAdapter.CancelOrderAsync(orderId);

            // Check if order was cancelled
            Assert.DoesNotContain((await HPPrivateAdapter.GetActiveOrdersAsync()),
                o => o.Id == orderId);
        }
    }
}
