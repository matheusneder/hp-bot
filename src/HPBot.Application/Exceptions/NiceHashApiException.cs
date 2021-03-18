using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    public abstract class NiceHashApiException : Exception
    {
        protected NiceHashApiException()
        {
        }

        protected NiceHashApiException(string message) : base(message)
        {
        }

        protected NiceHashApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NiceHashApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
