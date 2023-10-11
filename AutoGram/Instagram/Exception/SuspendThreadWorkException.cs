using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class SuspendThreadWorkException : System.Exception
    {
        public SuspendThreadWorkException()
        {
        }

        public SuspendThreadWorkException(string message) : base(message)
        {
        }

        public SuspendThreadWorkException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected SuspendThreadWorkException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
