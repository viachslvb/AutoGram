using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class SuspendTaskException : System.Exception
    {
        public SuspendTaskException()
        {
        }

        public SuspendTaskException(string message) : base(message)
        {
        }

        public SuspendTaskException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected SuspendTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
