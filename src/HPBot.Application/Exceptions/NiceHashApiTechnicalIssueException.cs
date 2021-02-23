using System;
using System.Runtime.Serialization;

namespace HPBot.Application.Exceptions
{
    public abstract class NiceHashApiTechnicalIssueException : NiceHashApiException
    {
        protected NiceHashApiTechnicalIssueException()
        {
        }

        protected NiceHashApiTechnicalIssueException(string message) : base(message)
        {
        }

        protected NiceHashApiTechnicalIssueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected NiceHashApiTechnicalIssueException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
