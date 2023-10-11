using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Storage.Model
{
    class ProxySettings
    {
        public bool UseProxyFromFile;
        public bool DontDisableEvenIfItDoesntWork;
        public bool ChangeForEachAccount;
        public bool ChangeIfEmptyCaption;
    }
}
