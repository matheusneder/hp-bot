using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace HPBot.Tests
{
    public class NiceHashApiAdapterTests
    {
        [Fact]
        public async Task OrderRefill_Success()
        {
            HashpowerMarketPrivateAdapter nhAdapter = CreateHashpowerMarketPrivateAdapter();

            await nhAdapter.RefillOrder("1ca8aaed-c07a-4e91-baaa-f03c02d68f94", 0.005F);
        }

        private static HashpowerMarketPrivateAdapter CreateHashpowerMarketPrivateAdapter()
        {
            NiceHashConfiguration configuration = NiceHashConfiguration
                .ReadFromNiceHashConfigJsonFile("test");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            HttpClient httpClient = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy()
                {
                    Address = new Uri("http://localhost:8888")
                }
            })
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            var nhClient = new NiceHashApiPersonedClient(httpClient, configuration, loggerFactory);
            var nhAdapter = new HashpowerMarketPrivateAdapter(nhClient);

            return nhAdapter;
        }

        [Fact]
        public async Task GetActiveOrders_Success()
        {
            HashpowerMarketPrivateAdapter nhAdapter = CreateHashpowerMarketPrivateAdapter();

            var orders = await nhAdapter.GetActiveOrdersAsync();

            Assert.Equal("1ca8aaed-c07a-4e91-baaa-f03c02d68f94", orders.First().Id);
        }
    }
}
