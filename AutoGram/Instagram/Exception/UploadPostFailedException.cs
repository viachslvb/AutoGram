using System.Runtime.Serialization;

namespace AutoGram.Instagram.Exception
{
    class UploadPostFailedException : System.Exception
    {
        public UploadPostFailedException()
        {
        }

        public UploadPostFailedException(string message) : base(message)
        {
        }

        public UploadPostFailedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected UploadPostFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
