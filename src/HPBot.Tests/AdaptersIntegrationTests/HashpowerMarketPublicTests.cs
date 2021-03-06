using HPBot.Application.Adapters;
using HPBot.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests.AdaptersIntegrationTests
{
    public class HashpowerMarketPublicTests
    {
        private HashpowerMarketPublicAdapter HPPublicAdapter =>
            new HashpowerMarketPublicAdapter(Helpers.NiceHashApiClient);

        [Fact]
        public async Task GetCurrentFixedPrice_FixedOrderPriceQuerySpeedLimitTooBig_Error()
        {
            // At time this test was written, nicehash test environment always raise this error

            var ex = await Assert.ThrowsAsync<GetCurrentFixedPriceException>(async () => await HPPublicAdapter
                .GetCurrentFixedPriceAsync("USA", 0.01F));

            Assert.Equal(GetCurrentFixedPriceException.GetCurrentFixedPriceErrorReason.FixedOrderPriceQuerySpeedLimitTooBig,
                ex.Reason);
        }
    }
}
