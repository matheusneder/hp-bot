using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    // currently used only for type: MARKET; side: SELL 
    public class ExchangeResultDto
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

        public string orderId { get; set; }
        public float origQty { get; set; } // ETH amount to sell
        public float executedQty { get; set; } // ETH amount sold
        public float executedSndQty { get; set; } // BTC amount received
        public string state { get; set; }
        public long lastResponseTime { get; set; }
    }
}
