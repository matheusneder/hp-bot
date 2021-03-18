using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    //{
    //  "list": [
    //    {
    //      "id": "faccd1c4-dbee-49e3-85a4-1dfe4450fae2",
    //      "created": 1612803251657,
    //      "currency": {
    //        "enumName": "ETH",
    //        "description": "ETH"
    //      },
    //      "amount": "0.11988485",
    //      "metadata": "{\"txid\":\"0x69661627b53b904268dc6004ccb1f6db889f401c7a755ac7138f90a7dfc07036:0\"}",
    //      "accountType": {
    //        "enumName": "USER",
    //        "description": "USER"
    //      },
    //      "address": "0x7eddb8cd10e43dace41e7a5ce0f5e644b952bcd6",
    //      "status": {
    //        "enumName": "COMPLETED",
    //        "description": "COMPLETED"
    //      },
    //      "confirmations": 41,
    //      "minConfirmations": 40,
    //      "meta": {
    //        "txid": "0x69661627b53b904268dc6004ccb1f6db889f401c7a755ac7138f90a7dfc07036:0",
    //        "fee": null,
    //        "error": null,
    //        "rejectedReason": null,
    //        "oldDeposit": null,
    //        "message": null,
    //        "coin": null
    //      }
    //    }
    //  ],
    //  "pagination": {
    //    "size": 10,
    //    "page": 0,
    //    "totalPageCount": 1
    //  }
    //}

    public class ListDepositResultDto
    {
        public class ItemDto
        {
            public string id { get; set; }
            public long created { get; set; }
            public string amount { get; set; }
            public CurrencyDto currency { get; set; }

            public class CurrencyDto 
            { 
                public string description { get; set; }
                public string enumName { get; set; }
            }

        }

        public IEnumerable<ItemDto> list { get; set; }
        
    }
}
