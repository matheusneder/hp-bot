using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Models
{
    public class ListOrderResultItem : Order
    {
        /// <summary>
        /// Total contracted including refills
        /// </summary>
        public float AmountBtc { get; set; }

        /// <summary>
        /// Total contracted including refills minus taxes
        /// </summary>
        public float AvailableAmountBtc { get; set; }
        
        /// <summary>
        /// Spent amount (not refundable)
        /// </summary>
        public float SpentWithoutTaxesAmountBtc { get; set; }

        public float TaxesAmountBtc => AmountBtc - AvailableAmountBtc - SpentWithoutTaxesAmountBtc;

        public float RemainAmountBtc => AvailableAmountBtc - SpentWithoutTaxesAmountBtc;

        public bool IsRunning { get; set; }
        /// <summary>
        /// Estimated amount of time till order balance get fully consumed
        /// </summary>
        public int EstimateDurationInSeconds { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
