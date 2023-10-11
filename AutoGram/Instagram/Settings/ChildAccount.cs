using xNet;

namespace AutoGram.Instagram.Settings
{
    class ChildAccount
    {
        public ChildAccount()
        {
            CreatedAt = Utils.DateTimeNowTotalSeconds;
        }

        public string Username;

        public string AccountId;

        public string Uuid;

        public string DeviceId;

        public string PhoneId;

        public string AdvertisingId;

        public string RankToken;

        public string SessionId;

        public string ClientSessionId;

        public string DeviceString;

        public string UserAgent;

        public CookieDictionary Cookies = new CookieDictionary();

        public bool IsCreatedSuccessfully;

        public long CreatedAt;
    }
}
