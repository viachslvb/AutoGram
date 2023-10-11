using AutoGram.Instagram.Response.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Helpers
{
    internal static class DirectThreadHelper
    {
        private static readonly List<string> _welcomeWords = new List<string>()
        {
            "hey", "hi", "hello"
        };

        public static bool IsWelcomeMessage(this IEnumerable<ThreadItemModel> messages)
        {
            foreach (var message in messages)
            {
                if (message.ItemType == "text")
                {
                    if (_welcomeWords.Any(w => message.Text.ToLower().Contains(w)))
                        return true;
                }
            }

            return false;
        }

        public static bool IsMediaMessage(this IEnumerable<ThreadItemModel> messages)
        {
            foreach (var message in messages)
            {
                return message.IsMedia() || message.IsRavenMedia() || message.IsReelShare();
            }

            return false;
        }
    }
}
