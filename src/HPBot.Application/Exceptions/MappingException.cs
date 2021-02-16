using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    /// <summary>
    /// Exception to be thrown when there are no mappings for a specific property (usually an enum).
    /// NOTE: This is not an application exception, it's a fatal error (need to fix mappings).
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        public MappingException(Type type, string propertyName, object rawValue)
        {
            Type = type;
            PropertyName = propertyName;
            RawValue = rawValue;
        }

        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Type Type { get; }
        public string PropertyName { get; }
        public object RawValue { get; }

        public override string Message => $"Could not convert the raw value of '{Convert.ToString(RawValue, CultureInfo.InvariantCulture)}' " +
            $"to fill {PropertyName} of {Type.FullName}.";
    }

    [Serializable]
    public class MappingException<T> : MappingException
    {
        public MappingException(string propertyName, object rawValue) : 
            base(typeof(T), propertyName, rawValue)
        {
        }

        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
