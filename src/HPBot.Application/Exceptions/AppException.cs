using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public abstract class AppException : Exception
    {
        protected AppException()
        {

        }

        protected AppException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
