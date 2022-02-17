using System;
using System.Runtime.Serialization;

namespace OQAS.XeroIntegration.Objects
{
    [Serializable]
    public class XeroRequestException : Exception
    {
        public XeroRequestException() { }
        public XeroRequestException(string message) : base(message) { }
        public XeroRequestException(string message, Exception innerException) : base(message, innerException) { }
        protected XeroRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
