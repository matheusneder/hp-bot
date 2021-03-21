using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public class HashpowerMarketPublicAdapter : IHashpowerMarketPublicAdapter
    {
        public INiceHashApiClient Client { get; set; }

        public HashpowerMarketPublicAdapter(NiceHashApiClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <exception cref="GetCurrentFixedPriceException"></exception>
        public async Task<FixedPriceResult> GetCurrentFixedPriceAsync(string market, float speedLimitThs)
        {
            try
            {
                var dto = await Client.PostAsync<FixedPriceResultDto>("/main/api/v2/hashpower/orders/fixedPrice",
                    new
                    {
                        limit = speedLimitThs,
                        market = market,
                        algorithm = "DAGGERHASHIMOTO"
                    });

                return new FixedPriceResult()
                {
                    FixedPriceBtc = float.Parse(dto.fixedPrice, CultureInfo.InvariantCulture.NumberFormat),
                    MaxSpeedThs = float.Parse(dto.fixedMax, CultureInfo.InvariantCulture.NumberFormat)
                };
            }
            catch (NiceHashApiClientException e)
            {
                if (e.HttpStatusCode == HttpStatusCode.BadRequest)
                {
#pragma warning disable S1066 // Collapsible "if" statements should be merged
                    if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5012))
#pragma warning restore S1066 // Collapsible "if" statements should be merged
                    {
                        throw new GetCurrentFixedPriceException(GetCurrentFixedPriceException.GetCurrentFixedPriceErrorReason.FixedOrderPriceQuerySpeedLimitTooBig);
                    }
                }

                throw new ErrorMappingException(nameof(GetCurrentFixedPriceAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }
    }
}
