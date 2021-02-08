using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    public class CreateOrderResultDto
    {
        //{
        //   "id":"e31f1ec8-0314-4ffb-9c9d-868becf7cc9c",
        //   "createdTs":"2021-02-03T23:19:28.355660Z",
        //   "updatedTs":"2021-02-03T23:19:28.390274Z",
        //   "requestId":"ed5efe6e-011d-4913-9196-b64aa2de23a1",
        //   "type":{
        //      "code":"FIXED",
        //      "description":"Fixed"
        //   },
        //   "market":"EU",
        //   "algorithm":{
        //      "algorithm":"DAGGERHASHIMOTO",
        //      "title":"DaggerHashimoto",
        //      "enabled":true,
        //      "order":20
        //   },
        //   "status":{
        //      "code":"ACTIVE",
        //      "description":"Active"
        //   },
        //   "price":"3.0638",
        //   "limit":"0.01",
        //   "amount":"0.001",
        //   "availableAmount":"0.0009506",
        //   "payedAmount":"0",
        //   "alive":false,
        //   "startTs":"2021-02-03T23:19:28.355346Z",
        //   "endTs":"2021-02-04T23:19:28.355346Z",
        //   "pool":{
        //      "id":"d8137d56-da26-428c-af5b-73138d361cf8",
        //      "name":"2miners-eu-eth-nicehash",
        //      "algorithm":"DAGGERHASHIMOTO",
        //      "stratumHostname":"eth.2miners.com",
        //      "stratumPort":2020,
        //      "username":"0x7eddb8cd10e43dace41e7a5ce0f5e644b952bcd6",
        //      "password":"x",
        //      "inMoratorium":false
        //   },
        //   "organizationId":"e48e6cc0-30ab-462e-ae17-0e6d27891fee",
        //   "creatorUserId":"d62c25e1-2a82-4708-a9c8-8018783711aa",
        //   "rigsCount":0,
        //   "acceptedCurrentSpeed":"0",
        //   "displayMarketFactor":"TH",
        //   "marketFactor":"1000000000000",
        //   "estimateDurationInSeconds":0
        //}
        public string id { get; set; }
        public string price { get; set; }
        public DateTimeOffset endTs { get; set; } // order life limit

        private string _marketFactor;

        public string marketFactor
        {
            get => _marketFactor;
            set
            {
                _marketFactor = value;

                if (_marketFactor != "1000000000000")
                {
                    throw new InvalidOperationException($"Expected marketFactor = 1000000000000 but {_marketFactor}");
                }
            }
        }
    }
}
