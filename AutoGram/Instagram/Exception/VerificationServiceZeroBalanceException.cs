using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class VerificationServiceZeroBalanceException : System.Exception
    {
        public VerificationServiceZeroBalanceException()
        {
        }

        public VerificationServiceZeroBalanceException(string message) : base(message)
        {
        }

        public VerificationServiceZeroBalanceException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationServiceZeroBalanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
