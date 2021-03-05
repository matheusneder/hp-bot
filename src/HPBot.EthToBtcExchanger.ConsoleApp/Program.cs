using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
            ExchangePrivateAdapter exchangePrivateAdapter = new ExchangePrivateAdapter(nhClient);
            WalletPrivateAdapter walletPrivateAdapter = new WalletPrivateAdapter(nhClient);
            var ethToBtcEchangeService = new EthToBtcExchangeService(exchangePrivateAdapter, walletPrivateAdapter, loggerFactory);

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
