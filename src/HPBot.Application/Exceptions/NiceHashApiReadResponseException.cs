using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class NiceHashApiReadResponseException : NiceHashApiHttpTransportException
    {
        public NiceHashApiReadResponseException(Exception innerException) : 
            base("An error has occured while reading response from NiceHash API, see InnerException for details.", innerException)
        {

        }
        protected NiceHashApiReadResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
