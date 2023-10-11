using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class EmptyPostCaptionException : System.Exception
    {
        public EmptyPostCaptionException()
        {
        }

        public EmptyPostCaptionException(string message) : base(message)
        {
        }

        public EmptyPostCaptionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected EmptyPostCaptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
