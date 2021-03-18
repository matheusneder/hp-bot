using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class NiceHashApiSendRequestException : NiceHashApiHttpTransportException
    {
        public NiceHashApiSendRequestException(Exception innerException) : 
            base("An error has occured while sending request to NiceHash API, see InnerException for details.", innerException)
        {
        }

        protected NiceHashApiSendRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
