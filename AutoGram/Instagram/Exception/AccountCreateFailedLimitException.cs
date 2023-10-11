using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class AccountCreateFailedLimitException : System.Exception
    {
        public AccountCreateFailedLimitException()
        {
        }

        public AccountCreateFailedLimitException(string message) : base(message)
        {
        }

        public AccountCreateFailedLimitException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected AccountCreateFailedLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
