using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class CancelOrderException : AppException
    {
        public CancelOrderException(string orderId, int httpStatusCode, string rawResponseText) : base()
        {
            OrderId = orderId;
            HttpStatusCode = httpStatusCode;
            RawResponseText = rawResponseText;
        }

        protected CancelOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string OrderId { get; }
        public int HttpStatusCode { get; }
        public string RawResponseText { get; }

        public override string Message => $"Error cancelling order #{OrderId}. HTTP status {HttpStatusCode}; ResponseText: '{RawResponseText}'";
    }
}
