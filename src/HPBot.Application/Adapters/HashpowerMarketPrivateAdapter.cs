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
    public class HashpowerMarketPrivateAdapter : IHashpowerMarketPrivateAdapter
    {
        public INiceHashApiClient Client { get; set; }

        public HashpowerMarketPrivateAdapter(NiceHashApiPersonedClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <exception cref="OrderCreationException" />
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
                    Expires = dto.endTs,
                    MarketFactor = float.Parse(dto.marketFactor, CultureInfo.InvariantCulture.NumberFormat)
                };
            }
            catch (NiceHashApiClientException e)
            {
                switch (e.HttpStatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Conflict:
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5054)) // Generic Error
                            throw new NiceHashApiServerException(e.HttpStatusCode, e.NiceHashApiErrorDto, null);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5056))
                            throw new OrderCreationException(OrderCreationException.CreateOrderErrorReason.PriceChanged);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 3001))
                            throw new OrderCreationException(OrderCreationException.CreateOrderErrorReason
                                .InsufficientBalanceInAccount);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 5067))
                            throw new OrderCreationException(OrderCreationException.CreateOrderErrorReason
                                .OrderAmountTooSmall);
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
            catch (NiceHashApiClientException e)
            {
                // TODO: Map undocumented errors 
                throw new ErrorMappingException(nameof(CancelOrderAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }

        /// <exception cref="OrderRefillException"></exception>
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
                            throw new OrderRefillException(id, amountBtc, OrderRefillException.RefillOrderErrorReason
                                .RefillOrderAmountBelowMinimalOrderAmount);
                        if (e.NiceHashApiErrorDto.errors.Any(e => e.code == 3001))
                            throw new OrderRefillException(id, amountBtc, OrderRefillException.RefillOrderErrorReason
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
                        Expires = i.endTs,
                        CreatedAt = i.startTs,
                        EstimateDurationInSeconds = i.estimateDurationInSeconds,
                        SpentWithoutTaxesAmountBtc = float.Parse(i.payedAmount, CultureInfo.InvariantCulture.NumberFormat),
                        PriceBtc = float.Parse(i.price, CultureInfo.InvariantCulture.NumberFormat),
                        MarketFactor = float.Parse(i.marketFactor, CultureInfo.InvariantCulture.NumberFormat)
                    }
                );
            }
            catch (NiceHashApiClientException e)
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
                    Expires = dto.endTs,
                    PriceBtc = float.Parse(dto.price, CultureInfo.InvariantCulture.NumberFormat),
                    Status = dto.status.code,
                    MarketFactor = float.Parse(dto.marketFactor, CultureInfo.InvariantCulture.NumberFormat)
                };
            }
            catch (NiceHashApiClientException e)
            {
                if (e.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                // TODO: Map undocumented errors if any
                throw new ErrorMappingException(nameof(GetOrderByIdAsync), e.HttpStatusCode, e.NiceHashApiErrorDto);
            }
        }
    }
}
