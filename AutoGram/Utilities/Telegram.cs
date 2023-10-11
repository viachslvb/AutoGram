using System;
using System.Threading;
using System.Windows;
using xNet;

namespace AutoGram
{
    enum TelegramNotification
    {
        InstagramAi,
        ServerRoom
    }

    class Telegram
    {
        // InstagramAI
        private const string Token = "somedata";
        private const string Id = "somedata";

        // Server Room
        private const string ServerRoomToken = "somedata";


        private static readonly object Locker = new object();

        public static void SendMessage(string message, TelegramNotification telegramNotification = TelegramNotification.InstagramAi)
        {
            var e = 0;
            while (true)
            {
                try
                {
                    using (HttpRequest request = new HttpRequest())
                    {
                        request.Reconnect = true;
                        request.IgnoreProtocolErrors = true;

                        request.AddUrlParam("chat_id", Id);
                        request.AddUrlParam("text", message);

                        var token = telegramNotification == TelegramNotification.InstagramAi
                            ? Token
                            : ServerRoomToken;

                        request.Get($"https://api.telegram.org/bot{token}/sendMessage");
                    }

                    break;
                }
                catch (HttpException ex)
                {
                    CheckConnection();
                }
                catch (Exception ex)
                {
                    e++;

                    if (e > 5)
                    {
                        MessageBox.Show($"{ex.Message} in {ex.Source}. StackTrace: {ex.StackTrace}");
                        return;
                    }
                }
            }
        }

        private static void CheckConnection()
        {
            lock (Locker)
            {
                while (!Proxy.CheckConnection(null))
                {
                    Notification.Show(NotificationException.BadConnection);
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
