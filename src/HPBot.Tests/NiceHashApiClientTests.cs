using HPBot.Application;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests
{
    public class NiceHashApiClientTests
    {
        [Fact]
        public async Task CreateHashPowerOrder_Success()
        {
            NiceHashConfiguration nhConfiguration = NiceHashConfiguration
                .ReadFromNiceHashConfigJsonFile("test");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var nhClient = new NiceHashApiClient(nhConfiguration, loggerFactory);

            await nhClient.PostAsync("/main/api/v2/hashpower/order",
                new
                {
                    market = "USA",
                    algorithm = "DAGGERHASHIMOTO",
                    displayMarketFactor = "TH",
                    marketFactor = 1000000000000,
                    amount = 0.005,
                    price = 2.7,
                    poolId = "bb5004a4-ca8f-456b-a485-dcdb21f2e886",
                    limit = 0.01,
                    type = "STANDARD"
                });
        }
    }
}
