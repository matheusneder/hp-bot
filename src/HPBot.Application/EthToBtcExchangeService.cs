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
        private readonly NiceHashApiAdapter niceHashApi;
        private readonly ILogger logger;

        public EthToBtcExchangeService(NiceHashApiAdapter niceHashApi, ILoggerFactory loggerFactory)
        {
            this.niceHashApi = niceHashApi ?? throw new ArgumentNullException(nameof(niceHashApi));
            logger = loggerFactory?.CreateLogger<EthToBtcExchangeService>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<EthToBtcExchangeResult> PerformExchangeIfHasNewDeposit(DateTimeOffset since)
        {
            var deposits = await niceHashApi.GetEthDepositsAsync(since);

            if (deposits.Any())
            {
                var totalEth = deposits.Sum(d => d.Amount);
                var amountEthToExchange = totalEth * 0.99F;

                logger.LogInformation(
                    "There are {DepositCount} new deposits; Total amount is {TotalEth}; Going to exchange {AmountEthToExchange}",
                    deposits.Count(),
                    totalEth,
                    amountEthToExchange);

                var exchangeResult = await niceHashApi.EthToBtcExchangeAsync(amountEthToExchange);
                exchangeResult.LastDepositCreatedAt = deposits.Max(d => d.CreatedAt);

                logger.LogError( // TODO: fix log level
                    "Exchanged OrderId: {OrderId} :: {AmountEth} ETH -> {AmountBtc} BTC; State: {State}", 
                    exchangeResult.OrderId,
                    exchangeResult.AmountEthSold,
                    exchangeResult.AmountBtcReceived,
                    exchangeResult.State); 

                if(exchangeResult.AmountEthToSell != exchangeResult.AmountEthSold)
                {
                    logger.LogError( // TODO: fix log level
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
