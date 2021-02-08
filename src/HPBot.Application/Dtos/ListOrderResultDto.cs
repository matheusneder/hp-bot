using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    //{
    //  "list": [
    //    {
    //      "id": "1ca8aaed-c07a-4e91-baaa-f03c02d68f94",
    //      "availableAmount": "0.0145306",
    //      "payedAmount": "0",
    //      "endTs": "2021-02-16T11:02:41.058269Z",
    //      "updatedTs": "2021-02-06T11:02:41.357777Z",
    //      "estimateDurationInSeconds": 0,
    //      "type": {
    //        "code": "STANDARD",
    //        "description": "Standard"
    //      },
    //      "market": "USA",
    //      "algorithm": {
    //        "algorithm": "DAGGERHASHIMOTO",
    //        "title": "DaggerHashimoto",
    //        "enabled": true,
    //        "order": 20
    //      },
    //      "status": {
    //        "code": "ACTIVE",
    //        "description": "Active"
    //      },
    //      "price": "0.1",
    //      "limit": "0.01",
    //      "amount": "0.015",
    //      "displayMarketFactor": "TH",
    //      "marketFactor": "1000000000000",
    //      "alive": false,
    //      "startTs": "2021-02-06T11:02:41.058269Z",
    //      "pool": {
    //        "id": "bb5004a4-ca8f-456b-a485-dcdb21f2e886",
    //        "name": "teste01",
    //        "algorithm": "DAGGERHASHIMOTO",
    //        "stratumHostname": "us-eth.2miners.com",
    //        "stratumPort": 2020,
    //        "username": "0xb944f1422f0b37DF79e9FD46579550e6aCa3b992",
    //        "password": "x",
    //        "inMoratorium": false
    //      },
    //      "acceptedCurrentSpeed": "0",
    //      "rigsCount": 0,
    //      "organizationId": "53b0b2d1-f535-4681-b010-1419ad215fb0",
    //      "creatorUserId": "9c44bc3a-57da-421d-93e1-df335b56d6f1"
    //    }
    //  ]
    //}

    public class ListOrderResultDto
    {
        public IEnumerable<ItemDto> list { get; set; }

        public class ItemDto
        {
            public string id { get; set; }
            public string amount { get; set; }
            public string availableAmount { get; set; } 
            public string payedAmount { get; set; } 
            public DateTimeOffset endTs { get; set; } // order life limit
            public int estimateDurationInSeconds { get; set; }
            public bool alive { get; set; }
            public DateTimeOffset startTs { get; set; } // order created
            public string price { get; set; }
            
            private string _marketFactor;
            
            public string marketFactor 
            {
                get => _marketFactor;
                set
                {
                    _marketFactor = value;

                    if(_marketFactor != "1000000000000")
                    {
                        throw new InvalidOperationException($"Expected marketFactor = 1000000000000 but {_marketFactor}");
                    }
                } 
            }
        }
    }
}
