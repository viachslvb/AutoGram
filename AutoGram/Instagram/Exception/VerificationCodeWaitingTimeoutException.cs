using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class VerificationCodeWaitingTimeoutException : System.Exception
    {
        public VerificationCodeWaitingTimeoutException()
        {
        }

        public VerificationCodeWaitingTimeoutException(string message) : base(message)
        {
        }

        public VerificationCodeWaitingTimeoutException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationCodeWaitingTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
