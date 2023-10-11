using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class SomethingWrongException : System.Exception
    {
        public SomethingWrongException()
        {
        }

        public SomethingWrongException(string message) : base(message)
        {
        }

        public SomethingWrongException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected SomethingWrongException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
