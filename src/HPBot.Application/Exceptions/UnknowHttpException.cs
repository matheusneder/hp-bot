using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class UnknowHttpException : Exception
    {
        public UnknowHttpException(HttpResponseMessage httpResponse)
            : base(message: $"HTTP code {(int)httpResponse.StatusCode}, response text: " +
                httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult())
        {
            HttpResponseMessage = httpResponse;
        }

        protected UnknowHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HttpResponseMessage HttpResponseMessage { get; }        
    }
}
