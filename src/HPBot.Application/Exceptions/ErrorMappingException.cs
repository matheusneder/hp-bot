using HPBot.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace HPBot.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when there are no mappings for a specific error response messsage.
    /// NOTE: This is not an application exception, it's a fatal error (need to fix mappings).
    /// </summary>
    [Serializable]
    public class ErrorMappingException : Exception
    {
        public ErrorMappingException(string operation, HttpStatusCode httpStatusCode, object errorData)
        {
            Operation = operation;
            HttpStatusCode = httpStatusCode;
            ErrorData = errorData;
        }

        protected ErrorMappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Operation { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public object ErrorData { get; }

        public override string Message => $"Could not map response for '{Operation}'. HTTP status {HttpStatusCode}; " +
            $"ErrorData: '{JsonSerializer.Serialize(ErrorData)}'";
    }
}
