using System.Linq;
using System.Windows;
using Org.BouncyCastle.Ocsp;
using Renci.SshNet.Messages;
using xNet;

namespace AutoGram.Instagram
{
    class Client
    {
        public readonly Request.Request Request;
        private Proxy _proxy;

        public Client()
        {
            Request = new Request.Request
            {
                AllowAutoRedirect = true,
                IgnoreProtocolErrors = true,
                Reconnect = true,
                ReconnectLimit = 5,
                ReadWriteTimeout = 40000,
                ConnectTimeout = 40000,
                Cookies = new CookieDictionary(),
                KeepAlive = true
            };
        }

        public void SetProxy(Proxy proxy)
        {
            if (proxy == null) return;

            switch (proxy.Type)
            {
                case 0:
                    Request.Proxy = new HttpProxyClient(proxy.Host, proxy.Port);
                    break;
                case 1:
                    Request.Proxy = new Socks4ProxyClient(proxy.Host, proxy.Port);
                    break;
                case 2:
                    Request.Proxy = new Socks5ProxyClient(proxy.Host, proxy.Port);
                    break;
            }

            if (proxy.IsAuth)
            {
                Request.Proxy.Username = proxy.Username;
                Request.Proxy.Password = proxy.Password;

                Request.Proxy.ConnectTimeout = 40000;
                Request.Proxy.ReadWriteTimeout = 40000;
            }

            this._proxy = proxy;
        }

        public Proxy GetProxy() => this._proxy;

        public void RemoveProxy()
        {
            this._proxy = null;
            this.Request.Proxy = null;
        }

        public CookieDictionary CloneCookies()
        {
            var clonedCookies = new CookieDictionary();

            foreach (var cookie in Request.Cookies)
            {
                clonedCookies.Add(cookie.Key, cookie.Value);
            }

            return clonedCookies;
        }

        public void SetUserAgent(string userAgent) => Request.UserAgent = userAgent;

        public void ClearCookies() => Request.Cookies = new CookieDictionary();

        public void SetCookies(CookieDictionary cookies) => Request.Cookies = cookies;

        public void RemoveCookieByKey(string key)
        {
            Request.Cookies.Remove(key);
        }

        public CookieDictionary GetCookies() => Request.Cookies;

        public string GetCookieValue(string cookieKey)
        {
            return Request.Cookies.First(x => x.Key == cookieKey).Value;
        }

        public string GetToken()
        {
            foreach (var requestCookie in Request.Cookies)
            {
                if (requestCookie.Key == "csrftoken")
                    return Request.Cookies.First(x => x.Key == "csrftoken").Value;
            }

            return string.Empty;
        }

        public string GetUserId()
        {
            foreach (var requestCookie in Request.Cookies)
            {
                if (requestCookie.Key == "ds_user_id")
                    return Request.Cookies.First(x => x.Key == "ds_user_id").Value;
            }

            return string.Empty;
        }

        public string GetSessionId()
        {
            foreach (var requestCookie in Request.Cookies)
            {
                if (requestCookie.Key == "sessionid")
                    return Request.Cookies.First(x => x.Key == "sessionid").Value;
            }

            return string.Empty;
        }
    }
}
