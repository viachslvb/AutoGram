using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class AccountCreateSomethingWrongException : System.Exception
    {
        public AccountCreateSomethingWrongException()
        {
        }

        public AccountCreateSomethingWrongException(string message) : base(message)
        {
        }

        public AccountCreateSomethingWrongException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected AccountCreateSomethingWrongException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
