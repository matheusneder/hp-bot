using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
using HPBot.Application.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public class NiceHashApiAdapter
    {
        public NiceHashApiClient Client { get; set; }

        public NiceHashApiAdapter(NiceHashApiClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <exception cref="CreateOrderException" />
        public async Task<CreateOrderResult> CreateOrderAsync(string market, float ammoutBtc, float priceBtc, 
            float speedLimitThs, string poolId, string orderType)
        {
            try
            {
                var dto = await Client.PostAsync<CreateOrderResultDto>("/main/api/v2/hashpower/order",
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

                return new CreateOrderResult()
                {
                    Id = dto.id,
                    PriceBtc = float.Parse(dto.price, CultureInfo.InvariantCulture.NumberFormat),
                    CanLiveTill = dto.endTs
                };
            }
            catch(NiceHashApiClientException e)
            {
                switch (e.HttpStatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Conflict:
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5054))
                            throw new CreateOrderException(CreateOrderException.CreateOrderErrorReason.GenericError);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5056))
                            throw new CreateOrderException(CreateOrderException.CreateOrderErrorReason.PriceChanged);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 3001))
                            throw new CreateOrderException(CreateOrderException.CreateOrderErrorReason
                                .InsufficientBalanceInAccount);
                        break;
                }

                throw new ErrorMappingException(nameof(CreateOrderAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }

        public async Task CancelOrderAsync(string id)
        {
            try
            {
                await Client.DeleteAsync($"/main/api/v2/hashpower/order/{id}");
            }
            catch(NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors 
                throw new ErrorMappingException(nameof(CancelOrderAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
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
            catch(NiceHashApiClientException e)
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

        /// <exception cref="RefillOrderException"></exception>
        public async Task RefillOrder(string id, float amountBtc)
        {
            try
            {
                await Client.PostAsync($"/main/api/v2/hashpower/order/{id}/refill",
                    new
                    {
                        amount = amountBtc
                    });
            }
            catch (NiceHashApiClientException e)
            {
                switch (e.HttpStatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Conflict:
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5090))
                            throw new RefillOrderException(id, amountBtc, RefillOrderException.RefillOrderExceptionReason
                                .RefillOrderAmountBelowMinimalOrderAmount);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 3001))
                            throw new RefillOrderException(id, amountBtc, RefillOrderException.RefillOrderExceptionReason
                                .InsufficientBalanceInAccount);
                        break;
                }

                throw new ErrorMappingException(nameof(RefillOrder), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }

        // GET /main/api/v2/hashpower/myOrders?algorithm=DAGGERHASHIMOTO&status=ACTIVE&active=true&op=GT&limit=10&ts=1612550585737
        /// <summary>
        /// Note: Active order not necessarily running, check IsRunning property
        /// </summary>
        public async Task<IEnumerable<ListOrderResultItem>> GetActiveOrdersAsync()
        {
            var query = new Dictionary<string, object>()
            {
                { "algorithm", "DAGGERHASHIMOTO" },
                { "status", "ACTIVE" },
                { "active", "true" },
                { "op", "GT" },
                { "ts", 1 },
                { "limit", 15 }
            };

            try
            {
                var dto = await Client.GetAsync<ListOrderResultDto>("/main/api/v2/hashpower/myOrders", query);

                if (dto?.list == null)
                {
                    throw new InvalidOperationException("Null list were returned.");
                }

                return dto.list.Select(i =>
                    new ListOrderResultItem()
                    {
                        Id = i.id,
                        AmountBtc = float.Parse(i.amount, CultureInfo.InvariantCulture.NumberFormat),
                        IsRunning = i.alive,
                        AvailableAmountBtc = float.Parse(i.availableAmount, CultureInfo.InvariantCulture.NumberFormat),
                        CanLiveTill = i.endTs,
                        CreatedAt = i.startTs,
                        EstimateDurationInSeconds = i.estimateDurationInSeconds,
                        PayedAmountBtc = float.Parse(i.payedAmount, CultureInfo.InvariantCulture.NumberFormat),
                        PriceBtc = float.Parse(i.price, CultureInfo.InvariantCulture.NumberFormat)
                    }
                );
            }
            catch(NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetActiveOrdersAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }

        public async Task<OrderDetailResult> GetOrderByIdAsync(string id)
        {
            try
            {
                var dto = await Client.GetAsync<OrderDetailResultDto>($"/main/api/v2/hashpower/order/{id}");

                return new OrderDetailResult()
                {
                    Id = dto.id,
                    CanLiveTill = dto.endTs,
                    PriceBtc = float.Parse(dto.price, CultureInfo.InvariantCulture.NumberFormat),
                    Status = dto.status.code
                };
            }
            catch(NiceHashApiClientException e)
            {
                if(e.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetOrderByIdAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
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
            catch(NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetOrderByIdAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
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
            catch(NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetEthDepositsAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }
    }
}
