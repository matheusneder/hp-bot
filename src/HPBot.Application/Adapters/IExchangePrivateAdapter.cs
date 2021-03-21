using HPBot.Application.Models;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public interface IExchangePrivateAdapter
    {
        INiceHashApiClient Client { get; set; }

        Task<EthToBtcExchangeResult> EthToBtcExchangeAsync(float amountEth);
        Task<EthToBtcExchangeResult> ExchangeAsync(string market, float amount);
    }
}