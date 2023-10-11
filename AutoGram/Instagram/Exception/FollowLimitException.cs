using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class FollowLimitException : System.Exception
    {
        public FollowLimitException()
        {
        }

        public FollowLimitException(string message) : base(message)
        {
        }

        public FollowLimitException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected FollowLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
