using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Storage.Model
{
    class RegisterSettings
    {
        public bool RegisterAccounts;
        public bool SingleRegistration;
        public int PauseFrom;
        public int PauseTo;
        public bool ResetConnection;
        public int ResetConnectionEvery;
        public bool RandomUploadProfilePicture;
        public bool ShareToFeedProfilePicture;
    }
}
