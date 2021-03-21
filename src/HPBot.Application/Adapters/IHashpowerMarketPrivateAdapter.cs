using HPBot.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public interface IHashpowerMarketPrivateAdapter
    {
        INiceHashApiClient Client { get; set; }

        Task CancelOrderAsync(string id);
        Task<CreateOrderResult> CreateOrderAsync(string market, float ammoutBtc, float priceBtc, float speedLimitThs, string poolId, string orderType);
        Task<IEnumerable<ListOrderResultItem>> GetActiveOrdersAsync();
        Task<OrderDetailResult> GetOrderByIdAsync(string id);
        Task RefillOrder(string id, float amountBtc);
    }
}