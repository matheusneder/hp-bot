using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HPBot.OrderRefillerGuard.ConsoleApp
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
            var orderRefillerService = new OrderRefillerService(new TwoCryptoCalcAdapter(),
                hashpowerMarketPrivateAdapter, loggerFactory);
            var orderRefillerGuardService = new OrderRefillerGuardService(orderRefillerService, loggerFactory);

            await orderRefillerGuardService.StartAsync();
        }
    }
}
