using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HPBot.OrderCreationFlow.ConsoleApp
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

            var logger = loggerFactory.CreateLogger<Program>();

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
            HashpowerMarketPublicAdapter hashpowerMarketPublicAdapter = new HashpowerMarketPublicAdapter(nhClient);
            HashpowerMarketPrivateAdapter hashpowerMarketPrivateAdapter = new HashpowerMarketPrivateAdapter(nhClient);
            var orderCreationService = new OrderCreationService(hashpowerMarketPublicAdapter, hashpowerMarketPrivateAdapter, loggerFactory);
            var orderCancellationService = new OrderCancellationService(hashpowerMarketPrivateAdapter, loggerFactory);

            var orderLifecycleService = new OrderCreationFlowService(
                orderCreationService, orderCancellationService, hashpowerMarketPrivateAdapter, new TwoCryptoCalcAdapter(), loggerFactory);

            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            for (; ; )
            {
                try
                {
                    await orderLifecycleService.StartAsync(speedLimitThs, amountBtc);
                }
                catch(Exception e)
                {
                    logger.LogWarning(e, "Error on main loop!");
                }

                await Task.Delay(5000);
            }
        }
        
    }
}
