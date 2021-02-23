using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class CreateOrderException : AppException
    {
        public CreateOrderException(CreateOrderErrorReason reason)
        {
            Reason = reason;
        }

        protected CreateOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CreateOrderErrorReason Reason { get; }

        public enum CreateOrderErrorReason
        {
            InsufficientBalanceInAccount = 3001,
            GenericError = 5054,
            PriceChanged = 5056
        }

        public override string Message => $"Reason: {Reason}";
    }
}
