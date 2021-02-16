using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when there are no mappings for a specific error response messsage.
    /// NOTE: This is not an application exception, it's a fatal error (need to fix mappings).
    /// </summary>
    [Serializable]
    public class ErrorMappingException : Exception
    {
        public ErrorMappingException(string operation, int httpStatusCode, string rawResponseText)
        {
            Operation = operation;
            HttpStatusCode = httpStatusCode;
            RawResponseText = rawResponseText;
        }

        protected ErrorMappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Operation { get; }
        public int HttpStatusCode { get; }
        public string RawResponseText { get; }

        public override string Message => $"Could not map response. HTTP status {HttpStatusCode}; ResponseText: '{RawResponseText}'";
    }
}
