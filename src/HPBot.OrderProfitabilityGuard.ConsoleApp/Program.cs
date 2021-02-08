using HPBot.Application;
using Microsoft.Extensions.Logging;
using System;
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

            var nhClient = new NiceHashApiClient(configuration, loggerFactory);
            var nhAdapter = new NiceHashApiAdapter(nhClient);
            var orderCancellationService = new OrderCancellationService(nhAdapter, loggerFactory);
            
            var orderProfitabilityGuardService = new OrderProfitabilityGuardService(
                orderCancellationService, nhAdapter, new TwoCryptoCalcAdapter(), loggerFactory);

            await orderProfitabilityGuardService.StartAsync();
        }
    }
}
