using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class OrderRefillException : AppException
    {
        public OrderRefillException(string orderId, float amountBtc, RefillOrderErrorReason reason)
        {
            OrderId = orderId;
            AmountBtc = amountBtc;
            Reason = reason;
        }

        protected OrderRefillException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string OrderId { get; }
        public float AmountBtc { get; }
        public RefillOrderErrorReason Reason { get; }

        public enum RefillOrderErrorReason
        {
            InsufficientBalanceInAccount = 3001,
            RefillOrderAmountBelowMinimalOrderAmount = 5090
        }

        public override string Message => $"Reason: {Reason}; OrderId: {OrderId}; AmountBtc: {AmountBtc}";
    }
}
