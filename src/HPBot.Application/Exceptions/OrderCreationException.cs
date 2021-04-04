using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class OrderCreationException : AppException
    {
        public OrderCreationException(CreateOrderErrorReason reason)
        {
            Reason = reason;
        }

        protected OrderCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CreateOrderErrorReason Reason { get; }

        public enum CreateOrderErrorReason
        {
            InsufficientBalanceInAccount = 3001,
            PriceChanged = 5056,
            OrderAmountTooSmall = 5067,
            /// <summary>
            /// Blocked by business logic
            /// </summary>
            OrderCreationBlocked = 10068
        }

        public override string Message => $"Reason: {Reason}";
    }
}
