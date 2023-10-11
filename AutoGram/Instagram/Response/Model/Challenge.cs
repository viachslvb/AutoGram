using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Response.Model
{
    class Challenge
    {
        public string Url;
        public string Api_path;
        public string Hide_webview_header;
        public bool Lock;
        public string Logout;
        public string Native_flow;

        public string GetUrl()
        {
            return this.Api_path;
        }
    }
}
