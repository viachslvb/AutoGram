using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class IgnoreJsonErrorsException : System.Exception
    {
        public IgnoreJsonErrorsException()
        {
        }

        public IgnoreJsonErrorsException(string message) : base(message)
        {
        }

        public IgnoreJsonErrorsException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected IgnoreJsonErrorsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
