using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public Task<EthToBtcExchangeResult> EthToBtcExchangeAsync(float amountEth)
        {
            return ExchangeAsync("ETHBTC", amountEth);
        }

        //POST /exchange/api/v2/order?market=TETHTBTC&side=SELL&type=MARKET&quantity=0.89
        /// <exception cref="ExchangeException"></exception>
        public async Task<EthToBtcExchangeResult> ExchangeAsync(string market, float amount)
        {
            var query = new Dictionary<string, object>()
            {
                { "market", market },
                { "side", "SELL" },
                { "type", "MARKET" },
                { "quantity", amount }
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
                        throw new MappingException<EthToBtcExchangeResult>(nameof(EthToBtcExchangeResult.State), dto.state),
                    LastOrderResponseTime = DateTimeOffset.FromUnixTimeSeconds(0)
                        .AddTicks(
                            (long)
                            (TimeSpan.TicksPerSecond * (float)dto.lastResponseTime / 1000000F))
                };
            }
            catch (NiceHashApiClientException e)
            {
                switch (e.HttpStatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:

                        // Exhange api error response schema does not respect NiceHashApiErrorDto
                        // So its being deserialized from RawResponseText to NiceHashExchangeApiErrorDto here
                        // TODO: Let's see further implementations (if has) in order to think about generilize or not this contract

                        var errorDto = JsonSerializer.Deserialize<NiceHashExchangeApiErrorDto>(e.RawResponseText);

                        if (errorDto?.error?.status == 1219)
                            throw new ExchangeException(ExchangeException.ExchangeErrorReason.QuantityTooSmall, market, amount);
                        
                        break;
                }

                throw new ErrorMappingException(nameof(ExchangeAsync), e.HttpStatusCode, e.RawResponseText);
            }
        }
    }
}
