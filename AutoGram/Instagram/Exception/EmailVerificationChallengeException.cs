using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class EmailVerificationChallengeException : System.Exception
    {
        public EmailVerificationChallengeException()
        {
        }

        public EmailVerificationChallengeException(string message) : base(message)
        {
        }

        public EmailVerificationChallengeException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected EmailVerificationChallengeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
