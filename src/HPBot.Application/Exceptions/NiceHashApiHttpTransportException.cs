using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    public abstract class NiceHashApiHttpTransportException : NiceHashApiTechnicalIssueException
    {
        protected NiceHashApiHttpTransportException()
        {
        }

        protected NiceHashApiHttpTransportException(string message) : base(message)
        {
        }

        protected NiceHashApiHttpTransportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected NiceHashApiHttpTransportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
