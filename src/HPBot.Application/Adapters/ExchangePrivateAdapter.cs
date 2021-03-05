using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPBot.Application.Adapters
{
    public class ExchangePrivateAdapter
    {
        public NiceHashApiClient Client { get; set; }

        public ExchangePrivateAdapter(NiceHashApiPersonedClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        //POST /exchange/api/v2/order?market=TETHTBTC&side=SELL&type=MARKET&quantity=0.89
        public async Task<EthToBtcExchangeResult> EthToBtcExchangeAsync(float quantityEth)
        {
            var query = new Dictionary<string, object>()
            {
                { "market", "ETHBTC" },
                { "side", "SELL" },
                { "type", "MARKET" },
                { "quantity", quantityEth }
            };

            try
            {
                var dto = await Client.PostAsync<ExchangeResultDto>("/exchange/api/v2/order", query, null);

                return new EthToBtcExchangeResult()
                {
                    OrderId = dto.orderId,
                    AmountEthToSell = dto.origQty,
                    AmountEthSold = dto.executedQty,
                    AmountBtcReceived = dto.executedSndQty,
                    State = dto.state == "FULL" ?
                        EthToBtcExchangeResult.ExchangeState.Full :
                        throw new MappingException<EthToBtcExchangeResult>(nameof(EthToBtcExchangeResult.State), dto.state)
                };
            }
            catch (NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(EthToBtcExchangeAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }
    }
}
