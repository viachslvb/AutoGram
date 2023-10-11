using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class EmptySourceException : System.Exception
    {
        public EmptySourceException()
        {
        }

        public EmptySourceException(string message) : base(message)
        {
        }

        public EmptySourceException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected EmptySourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
