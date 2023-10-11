using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class SuspendExecutionException : System.Exception
    {
        public SuspendExecutionException()
        {
        }

        public SuspendExecutionException(string message) : base(message)
        {
        }

        public SuspendExecutionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected SuspendExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
