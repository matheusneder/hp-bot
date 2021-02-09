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
        public string State { get; set; } // TODO: Map it
        public DateTimeOffset LastDepositCreatedAt { get; set; }
    }
}
