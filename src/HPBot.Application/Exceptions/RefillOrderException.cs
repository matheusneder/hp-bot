using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class RefillOrderException : AppException
    {
        public RefillOrderException(string orderId, float amountBtc, RefillOrderExceptionReason reason)
        {
            OrderId = orderId;
            AmountBtc = amountBtc;
            Reason = reason;
        }

        public RefillOrderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RefillOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string OrderId { get; }
        public float AmountBtc { get; }
        public RefillOrderExceptionReason Reason { get; }

        public enum RefillOrderExceptionReason
        {
            InsufficientBalanceInAccount = 3001,
            RefillOrderAmountBelowMinimalOrderAmount = 5090

        }
    }
}
