using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoGram
{
    enum NotificationException
    {
        BadConnection,
        UnprofitableWork,
        AccountsEnded,
        ProxiesEnded
    }

    enum NotificationType
    {
        Desktop,
        Telegram
    }

    class Notification : IEquatable<Notification>
    {
        private readonly NotificationException _exception;
        private readonly NotificationType _type;
        private readonly DateTime _dateTime;

        private static readonly Object Lock = new object();
        private static readonly List<Notification> Notifications = new List<Notification>();

        private Notification(NotificationException exception, NotificationType type)
        {
            _exception = exception;
            _type = type;
            _dateTime = DateTime.Now;
        }

        public static void Show(NotificationException exception, string additionalMessage = null)
        {
            if (!Variables.IsSharedVersion && exception == NotificationException.AccountsEnded)
            {
                // Telegram Notification
                if (!IsFrequently(exception, NotificationType.Telegram))
                {
                    Template message = Template.GetTemplate(exception, NotificationType.Telegram);
                    if (message == null) return;

                    var telegramMessage = additionalMessage != null ? $"{message.Subject} {additionalMessage}" : message.Subject;

                    Telegram.SendMessage(telegramMessage);
                }
            }
        }

        private static bool IsFrequently(NotificationException exception, NotificationType type)
        {
            bool isFrequently = true;

            lock (Lock)
            {
                if (Notifications.Any(n => n._exception == exception && n._type == type))
                {
                    var frequency = type == NotificationType.Desktop
                        ? DateTime.Now.AddMinutes(Variables.NotificationDesktopFrequency)
                        : DateTime.Now.AddMinutes(Variables.NotificationTelegramFrequency);

                    var lastNotification = Notifications.Last(n => n._exception == exception && n._type == type);

                    if (lastNotification._dateTime < frequency)
                    {
                        isFrequently = false;

                        Notifications.Remove(lastNotification);
                        Notifications.Add(new Notification(exception, type));
                    }
                }
                else
                {
                    isFrequently = false;
                    Notifications.Add(new Notification(exception, type));
                }
            }

            return isFrequently;
        }

        public bool Equals(Notification other)
        {
            return other != null && (this._exception == other._exception && this._type == other._type && this._dateTime == other._dateTime);
        }

        class Template
        {
            private readonly NotificationException _exception;
            private readonly NotificationType _type;
            public string Subject;
            public string Body;

            private static readonly List<Template> Templates;

            private Template(NotificationException exception, NotificationType type, string subject, string body)
            {
                _exception = exception;
                _type = type;
                Subject = subject;
                Body = body;
            }

            static Template()
            {
                if (!Variables.IsSharedVersion)
                {
                    Templates = new List<Template>
                    {
                        new Template(NotificationException.AccountsEnded, NotificationType.Desktop,
                            "All accounts were used", "I'm waiting for accounts."),

                        new Template(NotificationException.AccountsEnded, NotificationType.Telegram,
                            "You are here? I need accounts.", ""),

                        new Template(NotificationException.ProxiesEnded, NotificationType.Desktop,
                            "Maybe you'll be interested", "All proxies were used."),

                        new Template(NotificationException.ProxiesEnded, NotificationType.Telegram,
                            "Damn it! I used all proxies. I'm forced to switch to a connection without a proxy.", ""),

                        new Template(NotificationException.BadConnection, NotificationType.Desktop,
                            "No Internet", "No internet connection or connection too slow."),

                        new Template(NotificationException.UnprofitableWork, NotificationType.Desktop,
                            "Instagram bans accounts", "Maybe it makes sense to wait.."),

                        new Template(NotificationException.UnprofitableWork, NotificationType.Telegram,
                            "Holly Fuck!! Do something!", ""),
                    };
                }
                else
                {
                    Templates = new List<Template>
                        {
                            new Template(NotificationException.AccountsEnded, NotificationType.Desktop,
                                "All accounts were used", "I'm waiting for accounts."),

                            new Template(NotificationException.AccountsEnded, NotificationType.Telegram,
                                "You are here? I need accounts.", ""),

                            new Template(NotificationException.ProxiesEnded, NotificationType.Desktop,
                                "Maybe you'll be interested", "All proxies were used."),

                            new Template(NotificationException.ProxiesEnded, NotificationType.Telegram,
                                "Damn it! I used all proxies. I'm forced to switch to a connection without a proxy.", ""),

                            new Template(NotificationException.BadConnection, NotificationType.Desktop,
                                "No Internet", "No internet connection or connection too slow."),

                            new Template(NotificationException.UnprofitableWork, NotificationType.Desktop,
                                "Instagram bans accounts", "Maybe it makes sense to wait.."),

                            new Template(NotificationException.UnprofitableWork, NotificationType.Telegram,
                                "Holly Fuck!! Do something!", ""),
                        };
                }
            }

            public static Template GetTemplate(NotificationException exception, NotificationType type)
            {
                if (Templates.Any(v => v._exception == exception && v._type == type))
                {
                    return Templates.Find(v => v._exception == exception && v._type == type);
                }

                return null;
            }
        }
    }
}
