using HPBot.Application;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HPBot.EthToBtcExchanger.ConsoleApp
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

            var nhClient = new NiceHashApiClient(configuration, loggerFactory);
            var nhAdapter = new NiceHashApiAdapter(nhClient);
            var ethToBtcEchangeService = new EthToBtcExchangeService(nhAdapter, loggerFactory);

            string lastDepositDataFile = "c:\\hp-data\\last-eth-deposit.dat";

            for (; ; )
            {
                try
                {
                    var lastDeposit = DateTimeOffset.FromUnixTimeMilliseconds(
                        long.Parse(File.ReadAllText(lastDepositDataFile)));
                    
                    var exchangeResult = await ethToBtcEchangeService
                        .PerformExchangeIfHasNewDeposit(lastDeposit);

                    if(exchangeResult != null)
                    {
                        File.WriteAllText(lastDepositDataFile, exchangeResult
                            .LastDepositCreatedAt.ToUnixTimeMilliseconds().ToString());
                    }
                }
                catch(Exception e)
                {
                    logger.LogWarning(e, "An error has occurred on HPBot.EthToBtcExchanger main loop.");
                }

                await Task.Delay(60000);
            }
        }
    }
}
