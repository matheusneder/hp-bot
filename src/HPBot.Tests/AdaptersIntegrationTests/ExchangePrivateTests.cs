using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests.AdaptersIntegrationTests
{
    public class ExchangePrivateTests
    {
        private ExchangePrivateAdapter ExchangePrivateAdapter =>
            new ExchangePrivateAdapter(Helpers.NiceHashApiPersonedClient);

        [Fact]
        public async Task Exchange_QuantityTooSmall_Error()
        {
            string market = "TETHTBTC";
            float sendAmound = 0.0001F;

            var ex = await Assert.ThrowsAsync<ExchangeException>(async () =>
                await ExchangePrivateAdapter.ExchangeAsync(market, sendAmound));

            Assert.Equal(ExchangeException.ExchangeErrorReason.QuantityTooSmall, ex.Reason);
            Assert.Equal(sendAmound, ex.Amount);
            Assert.Equal("TETHTBTC", ex.Market);
        }

        [Fact]
        public async Task Exchange_Success()
        {
            string market = "TETHTBTC";
            float sendAmound = 0.001F;
            float expectedReceiveAmount = sendAmound / 1000F; // TETHTBTC conversion
            var exchangeResult = await ExchangePrivateAdapter.ExchangeAsync(market, sendAmound);

            Assert.Equal(sendAmound, exchangeResult.AmountEthSold);
            Assert.Equal(sendAmound, exchangeResult.AmountEthToSell);
            Assert.Equal(expectedReceiveAmount, exchangeResult.AmountBtcReceived);
            Assert.Equal(EthToBtcExchangeResult.ExchangeState.Full, exchangeResult.State);
            Assert.True(exchangeResult.LastOrderResponseTime > DateTimeOffset.Now.AddMinutes(-5));
            Assert.True(exchangeResult.LastOrderResponseTime < DateTimeOffset.Now.AddMinutes(5));
            Assert.Matches(Helpers.NiceHashIdPattern, exchangeResult.OrderId);
        }
    }
}
