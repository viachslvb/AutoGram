using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class InactiveUserException : System.Exception
    {
        public InactiveUserException()
        {
        }

        public InactiveUserException(string message) : base(message)
        {
        }

        public InactiveUserException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected InactiveUserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
