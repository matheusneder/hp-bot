using HPBot.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class NiceHashApiClientException : NiceHashApiException
    {
        public HttpStatusCode HttpStatusCode { get; }
        public NiceHashApiErrorDto NiceHashApiErrorDto { get; }
        public string RawResponseText { get; }

        public NiceHashApiClientException(HttpStatusCode statusCode, NiceHashApiErrorDto niceHashApiErrorDto, string rawResponseText)
        {
            HttpStatusCode = statusCode;
            NiceHashApiErrorDto = niceHashApiErrorDto;
            RawResponseText = rawResponseText;
        }

        protected NiceHashApiClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => $"NiceHash responded client error code {HttpStatusCode}, see {nameof(NiceHashApiErrorDto)} for details.";
    }
}
