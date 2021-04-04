using HPBot.Application.Models;
using System.Threading.Tasks;

namespace HPBot.Application.Services
{
    public interface IOrderCreationService
    {
        Task<CreateOrderResult> TryOrderAsync(string market, string poolId, float maxPriceBtc, float amountBtc, float speedLimitThs);
    }
}