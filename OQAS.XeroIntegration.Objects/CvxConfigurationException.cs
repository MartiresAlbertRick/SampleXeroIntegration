using System;
using System.Runtime.Serialization;

namespace OQAS.XeroIntegration.Objects
{
    [Serializable]
    public class CvxConfigurationException : Exception
    {
        public CvxConfigurationException() { }
        public CvxConfigurationException(string message) : base(message) { }
        public CvxConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        protected CvxConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
