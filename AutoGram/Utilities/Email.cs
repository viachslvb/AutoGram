using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Limilabs.Proxy;
using MailKit.Net.Pop3;
using MimeKit;

namespace AutoGram
{
    public class Email
    {
        public string Username;
        public string Password;
        private string _previousCode;

        public Email(string username, string password)
        {
            Username = username;
            Password = Utils.TryParse(password, "([a-zA-Z0-9]{6,20})");
        }

        public VerificationCodeResponse GetVerificationCode()
        {
            var n = 0;
            while (n < 4)
            {
                try
                {
                    var messages = RetrieveMessages();

                    if (messages.Any())
                    {
                        var firstMessage = messages.Last().HtmlBody;
                        
                        var pattern = @"(?<=color:.565a5c.font.size.18px..><font.size=.6..)(\d+)(?=..font...p)";

                        if (Regex.IsMatch(firstMessage, pattern))
                        {
                            Match match = Regex.Match(firstMessage, pattern, RegexOptions.IgnoreCase);
                            var code = match.Value;

                            if (string.IsNullOrEmpty(_previousCode) ||
                                !string.IsNullOrEmpty(_previousCode) && _previousCode != code)
                            {
                                _previousCode = code;
                                return new VerificationCodeResponse { Code = code, Status = true };
                            }
                        }
                    }

                    Thread.Sleep(Settings.Basic.Limit.EmailTimeoutWaiting);
                    n++;
                }
                catch (Pop3ProtocolException)
                {
                    return new VerificationCodeResponse { Status = false };
                }
                catch (SocketException)
                {
                    Thread.Sleep(5000);
                    n++;
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                    n++;
                }
            }

            return new VerificationCodeResponse { Status = false };
        }

        private IList<MimeMessage> RetrieveMessages()
        {
            using (var client = new Pop3Client())
            {
                if (Settings.IsAdvanced && Settings.Advanced.VerifyEmailViaProxy)
                {
                    ProxyFactory factory = new ProxyFactory();
                    IProxyClient proxy = factory.CreateProxy(ProxyType.Http, Settings.Advanced.EmailVerificationProxy.Ip,
                        Settings.Advanced.EmailVerificationProxy.Port, Settings.Advanced.EmailVerificationProxy.Username,
                        Settings.Advanced.EmailVerificationProxy.Password);

                    Socket socket = proxy.Connect("pop.mail.ru", 995);

                    client.Connect(socket, "pop.mail.ru", 995);
                }
                else
                {
                    client.Connect("pop.mail.ru", 995, true);
                }

                client.Authenticate(Username, Password);

                var messages = client.GetMessages(0, client.Count);

                client.Disconnect(true);

                return messages;
            }
        }

        public class VerificationCodeResponse
        {
            public bool Status { get; set; }
            public string Code { get; set; }
        }
    }
}
