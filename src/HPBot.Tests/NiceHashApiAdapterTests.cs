using HPBot.Application;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            NiceHashApiAdapter nhAdapter = CreateNiceHashAdapter();

            await nhAdapter.RefillOrder("1ca8aaed-c07a-4e91-baaa-f03c02d68f94", 0.005F);
        }

        private static NiceHashApiAdapter CreateNiceHashAdapter()
        {
            NiceHashConfiguration nhConfiguration = NiceHashConfiguration
                .ReadFromNiceHashConfigJsonFile("test");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var nhClient = new NiceHashApiClient(nhConfiguration, loggerFactory);
            var nhAdapter = new NiceHashApiAdapter(nhClient);
            return nhAdapter;
        }

        [Fact]
        public async Task GetActiveOrders_Success()
        {
            NiceHashApiAdapter nhAdapter = CreateNiceHashAdapter();

            var orders = await nhAdapter.GetActiveOrdersAsync();

            Assert.Equal("1ca8aaed-c07a-4e91-baaa-f03c02d68f94", orders.First().Id);
        }
    }
}
