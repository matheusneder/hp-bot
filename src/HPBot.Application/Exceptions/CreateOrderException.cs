using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class CreateOrderException : AppException
    {
        public CreateOrderException(string message) : base(message)
        {
        }

        protected CreateOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
