using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class VerificationServiceErrorLimitException : System.Exception
    {
        public VerificationServiceErrorLimitException()
        {
        }

        public VerificationServiceErrorLimitException(string message) : base(message)
        {
        }

        public VerificationServiceErrorLimitException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationServiceErrorLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
