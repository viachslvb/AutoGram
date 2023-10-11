using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class ChallengeRequiredException : System.Exception
    {
        public ChallengeRequiredException()
        {
        }

        public ChallengeRequiredException(string message) : base(message)
        {
        }

        public ChallengeRequiredException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected ChallengeRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
