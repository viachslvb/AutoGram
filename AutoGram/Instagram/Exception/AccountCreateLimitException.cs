using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class AccountCreateLimitException : System.Exception
    {
        public AccountCreateLimitException()
        {
        }

        public AccountCreateLimitException(string message) : base(message)
        {
        }

        public AccountCreateLimitException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected AccountCreateLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
