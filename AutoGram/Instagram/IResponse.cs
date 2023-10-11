using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram
{
    interface IResponse
    {
        bool IsOk();
        bool IsMessage();
        string GetStatus();
        string GetMessage();
    }
}
