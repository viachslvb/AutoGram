using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Exception
{
    class TermsAcceptRequiredException : System.Exception
    {
        public TermsAcceptRequiredException()
        {
        }

        public TermsAcceptRequiredException(string message) : base(message)
        {
        }

        public TermsAcceptRequiredException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected TermsAcceptRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
