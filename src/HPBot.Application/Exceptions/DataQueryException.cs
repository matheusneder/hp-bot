using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class DataQueryException : AppException
    {
        public DataQueryException(DataQueryExceptionCause cause)
        {
            Cause = cause;
        }

        protected DataQueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DataQueryExceptionCause Cause { get; }

        public enum DataQueryExceptionCause
        {
            /// <summary>
            /// Tipically occur when NiceHash has no more hashpower available for new fixed orders. 
            /// This is an ephemeral condition; should be back soon
            /// </summary>
            FixedOrderPriceQuerySpeedLimitTooBig = 1
        }

        public override string Message => $"DataQueryException: {Cause}";
    }
}
