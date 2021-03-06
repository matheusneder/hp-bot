using HPBot.Application.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests.AdaptersIntegrationTests
{
    public class WalletPrivateTests
    {
        private WalletPrivateAdapter WalletPrivateAdapter =>
            new WalletPrivateAdapter(Helpers.NiceHashApiPersonedClient);

        /// <summary>
        /// Requires at least one TETH deposit in TETH NiceHash wallet
        /// </summary>
        [Fact]
        public async Task GetDeposits_Success()
        {
            string currency = "TETH";

            var deposits = await WalletPrivateAdapter
                .GetDepositsAsync(currency, DateTimeOffset.Parse("1970-01-01T00:00:00Z"));

            Assert.Contains(deposits, d => d.Currency == currency); // ensure there are at least one deposit

            Assert.Collection(deposits, 
                d => 
                {
                    Assert.True(d.Amount > 0.0F);
                    Assert.True(d.CreatedAt > DateTimeOffset.Parse("2020-01-01T00:00:00Z"));
                    Assert.Matches(Helpers.NiceHashIdPattern, d.Id);
                    Assert.Equal(currency, d.Currency);
                });
        }
    }
}
