using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using xNet;

namespace AutoGram
{
    class Proxy
    {
        public string Host;
        public int Port;
        public string Username;
        public string Password;

        public bool IsAuth;
        public int Type;

        private static readonly List<Proxy> Proxies;
        private static int _counter;

        private static readonly object Locker = new object();

        static Proxy()
        {
            ImportProxies(Proxies = new List<Proxy>());
        }

        public Proxy()
        {
        }

        public Proxy(string proxy)
        {
            Host = proxy.Split(':')[0];
            Port = int.Parse(proxy.Split(':')[1]);

            try
            {
                Username = proxy.Split(':')[2];
                Password = proxy.Split(':')[3];

                if (Username != "" && Password != "")
                    IsAuth = true;
                else
                    IsAuth = false;

                Type = 0;
            }
            catch (Exception)
            {
                IsAuth = false;
                Type = 1;
            }
        }

        private static void ImportProxies(List<Proxy> proxiesList)
        {
            var proxies = File.ReadAllLines(Variables.FileProxies);
            proxiesList.AddRange(proxies.Where(proxy => proxy != String.Empty).Select(proxy => new Proxy(proxy)));
        }

        public static void Update()
        {
            ImportProxies(Proxies);
        }

        public static bool Any()
        {
            return Proxies.Any();
        }

        public static Proxy Get()
        {
            if (_counter >= Proxies.Count)
                _counter = 0;

            Proxy proxy = Proxies[_counter];
            _counter++;

            return proxy;
        }

        public static void Delete(Proxy proxy)
        {
            Proxies.Remove(proxy);
        }

        public static void Set(Instagram.Instagram user)
        {
            if (Any())
            {
                user.SetProxy(Get());
                UIManager.ProxiesEnded(false);
            }
            else
            {
                Update();
                if (Any())
                    Set(user);

                user.RemoveProxy();

                Notification.Show(NotificationException.ProxiesEnded);
                UIManager.ProxiesEnded(true);
            }
        }

        public static bool CheckConnection(Proxy proxy)
        {
            bool throughProxy = proxy != null;

            try
            {
                using (var request = new HttpRequest())
                {
                    request.IgnoreProtocolErrors = true;
                    request[HttpHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                    if (throughProxy)
                    {
                        if (proxy.Type == 0)
                        {
                            request.Proxy = HttpProxyClient.Parse($"{proxy.Host}:{proxy.Port}");
                        }
                        else
                        {
                            request.Proxy = Socks4ProxyClient.Parse($"{proxy.Host}:{proxy.Port}");
                        }

                        if (proxy.IsAuth)
                        {
                            request.Proxy.Username = proxy.Username;
                            request.Proxy.Password = proxy.Password;
                        }

                        request.Proxy.ConnectTimeout = 15000;
                        request.Proxy.ReadWriteTimeout = 15000;
                    }

                    request.ConnectTimeout = 15000;
                    request.ReadWriteTimeout = 15000;

                    request.Get("https://i.instagram.com/api/v1/");
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void CheckConnection(Instagram.Instagram user, ref int n)
        {
            if (user.IsProxy())
            {
                if (!Settings.Basic.Proxy.DontDisableEvenIfItDoesntWork)
                {
                    lock (Locker)
                    {
                        if (!CheckConnection(null))
                        {
                            while (!CheckConnection(null))
                            {
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            //Delete(user.GetProxy());
                            user.Log($"Proxy invalid. Changed proxy.");
                            Set(user);
                        }
                    }
                }
                else
                {
                    var checkingStartTime = Utils.DateTimeNowTotalSeconds;

                    while (!CheckConnection(user.GetProxy()))
                    {
                        var nowTime = Utils.DateTimeNowTotalSeconds;

                        if (nowTime - checkingStartTime > 150)
                        {
                            user.Log("No proxy connection.");
                            Telegram.SendMessage($"No proxy connection. [{user.GetProxy().Host}]", TelegramNotification.ServerRoom);
                            checkingStartTime += 150;

                            if (user.Worker.IsSettings)
                            {
                                if (user.Worker.Settings.UseSshReconnect)
                                {
                                    ConsoleCommands.ResetSshConnection(user.Worker.Settings.SshClient);
                                }

                                if (user.Worker.Settings.UseTcpClientReconnect)
                                {
                                    ConsoleCommands.ResetNetworkThroughTcpClient(user.Worker.Settings
                                        .TcpClient);
                                    user.Log("Reconnecting through tcp client was successful.");
                                }
                            }
                        }

                        Thread.Sleep(5000);
                    }
                }
            }
            else
            {
                lock (Locker)
                {
                    while (!CheckConnection(null))
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
        }

        public static bool Equals(Proxy x, Proxy y)
        {
            if (x == y) return true;

            return x != null && y != null && x.Host == y.Host && x.Port == y.Port;
        }
    }
}
