using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class PhoneVerificationChallengeException : System.Exception
    {
        public PhoneVerificationChallengeException()
        {
        }

        public PhoneVerificationChallengeException(string message) : base(message)
        {
        }

        public PhoneVerificationChallengeException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected PhoneVerificationChallengeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
