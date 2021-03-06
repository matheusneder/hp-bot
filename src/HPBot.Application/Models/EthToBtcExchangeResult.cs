using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Models
{
    public class EthToBtcExchangeResult
    {
        public string OrderId { get; set; }
        public float AmountEthToSell { get; set; }
        public float AmountEthSold { get; set; }
        public float AmountBtcReceived { get; set; }
        public ExchangeState State { get; set; }
        public DateTimeOffset LastOrderResponseTime { get; set; }

        public enum ExchangeState
        {
            Full  = 1
        }
    }
}
