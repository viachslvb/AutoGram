using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Exception
{
    class DirectDeclineRequestException : System.Exception
    {
        public DirectDeclineRequestException()
        {
        }

        public DirectDeclineRequestException(string message) : base(message)
        {
        }

        public DirectDeclineRequestException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected DirectDeclineRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
