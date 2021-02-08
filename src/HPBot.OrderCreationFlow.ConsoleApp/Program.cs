using HPBot.Application;
using Microsoft.Extensions.Logging;
using System;
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

            var nhClient = new NiceHashApiClient(configuration, loggerFactory);
            var nhAdapter = new NiceHashApiAdapter(nhClient);
            var orderCreationService = new OrderCreationService(nhAdapter, loggerFactory);
            var orderCancellationService = new OrderCancellationService(nhAdapter, loggerFactory);

            var orderLifecycleService = new OrderCreationFlowService(
                orderCreationService, orderCancellationService, nhAdapter, new TwoCryptoCalcAdapter(), loggerFactory);

            float amountBtc = 0.001F;
            float speedLimitThs = 0.01F;

            await orderLifecycleService.StartAsync(speedLimitThs, amountBtc);
        }
        
    }
}
