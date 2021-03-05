using HPBot.Application.Adapters;
using HPBot.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class EthToBtcExchangeService
    {
        private readonly ILogger logger;
        private readonly ILogger notifier;
        private readonly ExchangePrivateAdapter exchangePrivateAdapter;
        private readonly WalletPrivateAdapter walletPrivateAdapter;

        public EthToBtcExchangeService(ExchangePrivateAdapter exchangePrivateAdapter, 
            WalletPrivateAdapter walletPrivateAdapter, ILoggerFactory loggerFactory)
        {           
            this.exchangePrivateAdapter = exchangePrivateAdapter ?? 
                throw new ArgumentNullException(nameof(exchangePrivateAdapter));

            this.walletPrivateAdapter = walletPrivateAdapter ?? 
                throw new ArgumentNullException(nameof(walletPrivateAdapter));
            
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            logger = loggerFactory.CreateLogger<EthToBtcExchangeService>();
            notifier = loggerFactory.CreateNotifier<EthToBtcExchangeService>();
        }

        public async Task<EthToBtcExchangeResult> PerformExchangeIfHasNewDeposit(DateTimeOffset since)
        {
            var deposits = await walletPrivateAdapter.GetEthDepositsAsync(since);

            if (deposits.Any())
            {
                var totalEth = deposits.Sum(d => d.Amount);
                var amountEthToExchange = totalEth * 0.99F;

                logger.LogInformation(
                    "There are {DepositCount} new deposits; Total amount is {TotalEth}; Going to exchange {AmountEthToExchange}",
                    deposits.Count(),
                    totalEth,
                    amountEthToExchange);

                var exchangeResult = await exchangePrivateAdapter.EthToBtcExchangeAsync(amountEthToExchange);
                exchangeResult.LastDepositCreatedAt = deposits.Max(d => d.CreatedAt);

                notifier.LogInformation(
                    "Exchanged OrderId: {OrderId} :: {AmountEth} ETH -> {AmountBtc} BTC; State: {State}",
                    exchangeResult.OrderId,
                    exchangeResult.AmountEthSold,
                    exchangeResult.AmountBtcReceived,
                    exchangeResult.State);

                if(exchangeResult.AmountEthToSell != exchangeResult.AmountEthSold)
                {
                    notifier.LogWarning(
                        "OrderId: {OrderId} :: AmountEthToSell ({AmountEthToSell}) != AmountEthSold ({AmountEthSold})",
                        exchangeResult.OrderId,
                        exchangeResult.AmountEthToSell,
                        exchangeResult.AmountEthSold);
                }

                return exchangeResult;
            }
            else
            {
                logger.LogInformation("There are no new ETH deposits");
            }

            return null;
        }
    }
}
