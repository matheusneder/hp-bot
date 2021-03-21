using HPBot.Application.Models;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public interface IHashpowerMarketPublicAdapter
    {
        INiceHashApiClient Client { get; set; }

        Task<FixedPriceResult> GetCurrentFixedPriceAsync(string market, float speedLimitThs);
    }
}