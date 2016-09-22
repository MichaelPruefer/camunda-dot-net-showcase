using System;
#if NET46
using System.Runtime.Serialization;
#endif

namespace CamundaClient
{
#if NET46
    [Serializable]
#endif
    public class EngineException : Exception
    {

        public EngineException() { }

        public EngineException(string message) : base(message) { }

        public EngineException(string message, Exception innerException) : base(message, innerException) { }

#if NET46
        protected EngineException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
