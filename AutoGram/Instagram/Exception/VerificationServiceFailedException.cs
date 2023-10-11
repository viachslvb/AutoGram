using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class VerificationServiceFailedException : System.Exception
    {
        public VerificationServiceFailedException()
        {
        }

        public VerificationServiceFailedException(string message) : base(message)
        {
        }

        public VerificationServiceFailedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationServiceFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
