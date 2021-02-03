using HPBot.Application.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class NiceHashAdapter
    {
        private readonly NiceHashApiClient nhClient;

        public NiceHashAdapter(NiceHashApiClient nhClient)
        {
            this.nhClient = nhClient ?? throw new ArgumentNullException(nameof(nhClient));
        }

        public async Task<CreateOrderResult> CreateOrderAsync(string market, float ammoutBtc, float priceBtc, float speedLimitThs, string poolId, string orderType)
        {
            var httpResponse = await nhClient.PostAsync("/main/api/v2/hashpower/order",
                new
                {
                    market = market,
                    algorithm = "DAGGERHASHIMOTO",
                    displayMarketFactor = "TH",
                    marketFactor = 1000000000000,
                    amount = ammoutBtc,
                    price = priceBtc,
                    poolId = poolId,
                    limit = speedLimitThs,
                    type = orderType
                });

            if(httpResponse.IsSuccessStatusCode)
            {
                return new CreateOrderResult(); // TODO: complete
            }
            else
            {
                switch (httpResponse.StatusCode)
                {
                    case (HttpStatusCode)409:
                        
                        // {"error_id":"cab5d79b-e0e3-44e7-9d02-4a4845428081","errors":[{"code":3001,"message":"Insufficient balance in account: (TBTC, USER, 53b0b2d1-f535-4681-b010-1419ad215fb0)"}]}            
                        throw new NotImplementedException(); // TODO: complete
                    default:
                        throw new NotImplementedException(); // TODO: complete
                }
            }
        }
    }
}
