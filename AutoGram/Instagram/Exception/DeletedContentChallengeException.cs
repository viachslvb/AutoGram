using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class DeletedContentChallengeException : System.Exception
    {
        public DeletedContentChallengeException()
        {
        }

        public DeletedContentChallengeException(string message) : base(message)
        {
        }

        public DeletedContentChallengeException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected DeletedContentChallengeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
