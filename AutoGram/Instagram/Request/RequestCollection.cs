using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Requests
{
    class RequestCollection
    {
        public Instagram User;

        public RequestCollection(Instagram instagram)
        {
            this.User = instagram;
        }
    }
}
