using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class SuspendAccountCreatingException : System.Exception
    {
        public SuspendAccountCreatingException()
        {
        }

        public SuspendAccountCreatingException(string message) : base(message)
        {
        }

        public SuspendAccountCreatingException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected SuspendAccountCreatingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
