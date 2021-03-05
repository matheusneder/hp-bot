using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public class WalletPrivateAdapter
    {
        public NiceHashApiClient Client { get; set; }

        public WalletPrivateAdapter(NiceHashApiPersonedClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<ListDepositResultItem>> GetEthDepositsAsync(DateTimeOffset since)
        {
            var query = new Dictionary<string, object>()
            {
                //statuses=COMPLETED&op=GT&timestamp=1612795336317&page=0&size=10
                { "statuses", "COMPLETED" },
                { "op", "GT" },
                { "timestamp", since.ToUnixTimeMilliseconds() },
                { "page", 0 },
                { "size", 10 }
            };

            try
            {
                var dto = await Client.GetAsync<ListDepositResultDto>("/main/api/v2/accounting/deposits/ETH", query);

                return dto.list.Select(i => new ListDepositResultItem()
                {
                    Id = i.id,
                    CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(i.created),
                    Amount = float.Parse(i.amount, CultureInfo.InvariantCulture.NumberFormat)
                });
            }
            catch (NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetEthDepositsAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }
    }
}
