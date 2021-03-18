using HPBot.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class NiceHashApiServerException : NiceHashApiTechnicalIssueException
    {
        public HttpStatusCode StatusCode { get; }
        public NiceHashApiErrorDto NiceHashApiErrorDto { get; }
        public string RawResponseText { get; }

        public NiceHashApiServerException(HttpStatusCode statusCode, NiceHashApiErrorDto niceHashApiErrorDto, string rawResponseText)
        {
            StatusCode = statusCode;
            NiceHashApiErrorDto = niceHashApiErrorDto;
            RawResponseText = rawResponseText;
        }

        protected NiceHashApiServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message => 
            $"NiceHash responded server error code {StatusCode}, " +
            $"see {nameof(NiceHashApiErrorDto)}/{nameof(RawResponseText)} for details.";
    }
}
