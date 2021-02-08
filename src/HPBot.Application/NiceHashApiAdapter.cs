﻿using HPBot.Application.Dtos;
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
        public async Task<CreateOrderResult> CreateOrderAsync(string market, float ammoutBtc, float priceBtc, float speedLimitThs, string poolId, string orderType)
        {
            var httpResponse = await Client.PostAsync("/main/api/v2/hashpower/order",
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
                string responseText = await httpResponse.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<CreateOrderResultDto>(responseText);

                return new CreateOrderResult()
                {
                    Id = dto.id,
                    PriceBtc = float.Parse(dto.price, CultureInfo.InvariantCulture.NumberFormat),
                    CanLiveTill = dto.endTs
                };
            }

            switch (httpResponse.StatusCode)
            {
                case (HttpStatusCode)400:
                case (HttpStatusCode)409:
                    var errorDto = JsonSerializer.Deserialize<NiceHashApiErrorDto>(await httpResponse.Content.ReadAsStringAsync());

                    if (errorDto.errors.Any(e => e.code == 5054))
                        throw new CreateOrderException("Error creating fixed order");

                    if (errorDto.errors.Any(e => e.code == 5056))
                        throw new CreateOrderException("Price changed");

                    if (errorDto.errors.Any(e => e.code == 3001))
                        throw new CreateOrderException("Insufficient balance in account");
                    
                    break;
            }

            throw new UnknowHttpException(httpResponse);

        }

        public async Task CancelOrderAsync(string id)
        {
            var httpResponse = await Client.DeleteAsync($"/main/api/v2/hashpower/order/{id}");

            if(!httpResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(); // TODO: completar
            }
        }

        public async Task<FixedPriceResult> GetCurrentFixedPrice(string market, float speedLimitThs)
        {
            var httpResponse = await Client.PostAsync("/main/api/v2/hashpower/orders/fixedPrice",
                new
                {
                    limit = speedLimitThs,
                    market = market,
                    algorithm = "DAGGERHASHIMOTO"
                });

            if (httpResponse.IsSuccessStatusCode)
            {
                string responseText = await httpResponse.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<FixedPriceResultDto>(responseText);

                return new FixedPriceResult()
                {
                    FixedPriceBtc = float.Parse(dto.fixedPrice, CultureInfo.InvariantCulture.NumberFormat),
                    MaxSpeedThs = float.Parse(dto.fixedMax, CultureInfo.InvariantCulture.NumberFormat)
                };
            }

            // HTTP 400 {"error_id":"04177a38-0a46-4c0a-800b-7af91976cf3c","errors":[{"code":5012,"message":"Hashpower order fixed speed limit is too big"}]}


            throw new InvalidOperationException(); // TODO: complete
        }

        public async Task RefillOrder(string id, float amountBtc)
        {
            var httpResponse = await Client.PostAsync($"/main/api/v2/hashpower/order/{id}/refill",
                new 
                {
                    amount = amountBtc
                });

            if (!httpResponse.IsSuccessStatusCode)
            {
                // HTTP 400: {"error_id":"3af7aaa4-73b6-4623-9eb5-126827be942b","errors":[{"code":5090,"message":"Refill order amount below minimal order amount"}]}
                // HTTP 409: {"error_id":"14cd594b-f7d0-400e-87eb-30ee27560f4d","errors":[{"code":3001,"message":"Insufficient balance in account: (TBTC, USER, 53b0b2d1-f535-4681-b010-1419ad215fb0)"}]}

                throw new InvalidOperationException(); // TODO: complete
            }
        }

        // GET /main/api/v2/hashpower/myOrders?algorithm=DAGGERHASHIMOTO&status=ACTIVE&active=true&op=GT&limit=10&ts=1612550585737
        /// <summary>
        /// Note: Active order not necessarily running, check IsRunning property
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ListOrderResultItem>> GetActiveOrders()
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

            var httpResult = await Client.GetAsync("/main/api/v2/hashpower/myOrders", query);

            if (httpResult.IsSuccessStatusCode)
            {
                var dto = JsonSerializer
                    .Deserialize<ListOrderResultDto>(await httpResult.Content.ReadAsStringAsync());

                if(dto?.list == null)
                {
                    throw new InvalidOperationException(); // TODO: complete
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

            throw new InvalidOperationException(); // TODO: complete
        }

        public async Task<OrderDetailResult> GetOrderById(string id)
        {
            var httpResult = await Client.GetAsync($"/main/api/v2/hashpower/order/{id}");

            if (httpResult.IsSuccessStatusCode)
            {
                var dto = JsonSerializer
                    .Deserialize<OrderDetailResultDto>(await httpResult.Content.ReadAsStringAsync());

                return new OrderDetailResult()
                {
                    Id = dto.id,
                    CanLiveTill = dto.endTs,
                    PriceBtc = float.Parse(dto.price, CultureInfo.InvariantCulture.NumberFormat),
                    Status = dto.status.code
                };
            }

            if(httpResult.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new InvalidOperationException(); // TODO: complete
        }

        //POST /exchange/api/v2/order?market=TETHTBTC&side=SELL&type=MARKET&quantity=0.89
        public Task EthToBtcExchangeAsync(float quantityEth)
        {
            // response:
            //{
            //  "orderId": "38b83e0f-f584-43ef-989f-ffd25f792805",
            //  "price": 0,
            //  "origQty": 0.89,
            //  "origSndQty": 0,
            //  "executedQty": 0.89,
            //  "executedSndQty": 0.00089,
            //  "type": "MARKET",
            //  "side": "SELL",
            //  "submitTime": 1612390009527571,
            //  "lastResponseTime": 1612390009674477,
            //  "state": "FULL"
            //}
            throw new NotImplementedException(); // TODO: implement!
        }
    }
}