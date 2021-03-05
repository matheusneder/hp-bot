using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HPBot.OrderProfitabilityGuard.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            NiceHashConfiguration configuration = NiceHashConfiguration
                .ReadFromNiceHashConfigJsonFile("production");
            
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TelegramLogProvider());
            });

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
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter = new HashpowerMarketPrivateAdapter(nhClient);
            var orderCancellationService = new OrderCancellationService(hashpowerMarketPrivateAdapter, loggerFactory);
            
            var orderProfitabilityGuardService = new OrderProfitabilityGuardService(
                orderCancellationService, hashpowerMarketPrivateAdapter, new TwoCryptoCalcAdapter(), loggerFactory);

            await orderProfitabilityGuardService.StartAsync();
        }
    }
}
