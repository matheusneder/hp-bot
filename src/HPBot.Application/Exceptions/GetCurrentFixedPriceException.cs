using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class GetCurrentFixedPriceException : AppException
    {
        public GetCurrentFixedPriceException(GetCurrentFixedPriceErrorReason reason)
        {
            Reason = reason;
        }

        protected GetCurrentFixedPriceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public GetCurrentFixedPriceErrorReason Reason { get; }

        public enum GetCurrentFixedPriceErrorReason
        {
            /// <summary>
            /// Tipically occur when NiceHash has no more hashpower available for new fixed orders. 
            /// This is an ephemeral condition.
            /// </summary>
            FixedOrderPriceQuerySpeedLimitTooBig = 1
        }

        public override string Message => $"Reason: {Reason}";
    }
}
