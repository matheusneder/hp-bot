using HPBot.Application;
using Microsoft.Extensions.Logging;
using System;
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

            var nhClient = new NiceHashApiClient(configuration, loggerFactory);
            var nhAdapter = new NiceHashApiAdapter(nhClient);
            var orderRefillerService = new OrderRefillerService(new TwoCryptoCalcAdapter(), 
                nhAdapter, loggerFactory);
            var orderRefillerGuardService = new OrderRefillerGuardService(orderRefillerService, loggerFactory);

            await orderRefillerGuardService.StartAsync();
        }
    }
}
