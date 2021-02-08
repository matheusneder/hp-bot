using HPBot.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests
{
    public class TwoCryptoCalcAdapterTests
    {
        [Fact]
        public async Task GetEthMiningProfabilityBtcAsync_Success()
        {
            var twoCryptoCalcAdapter = new TwoCryptoCalcAdapter();
            float btc = await twoCryptoCalcAdapter.GetEthMiningAverageRewardBtcAsync();

            Assert.True(btc > 0.1F);
        }
    }
}
