using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public interface IWalletPrivateAdapter
    {
        INiceHashApiClient Client { get; set; }

        Task<IEnumerable<ListDepositResultItem>> GetDepositsAsync(string currency, DateTimeOffset since);
        Task<IEnumerable<ListDepositResultItem>> GetEthDepositsAsync(DateTimeOffset since);
    }
}