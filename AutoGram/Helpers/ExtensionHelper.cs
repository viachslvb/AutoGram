using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Database.Direct;
using AutoGram.Instagram.Response.Direct;

namespace AutoGram.Helpers
{
    internal static class ExtensionHelper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string EncodeList(this long[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }

        public static string EncodeList(this string[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }

        public static string EncodeList(this List<long> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }

        public static string EncodeList(this List<string> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }

        public static string Encode(this long content)
        {
            return content.ToString().Encode();
        }

        public static string Encode(this string content)
        {
            return "\"" + content + "\"";
        }

        public static string EncodeRecipients(this long[] recipients)
        {
            return EncodeRecipients(recipients.ToList());
        }

        public static string EncodeRecipients(this List<long> recipients)
        {
            var list = new List<string>();
            foreach (var item in recipients)
                list.Add($"[{item}]");
            return string.Join(",", list);
        }

        public static List<DirectThreadItem> ConvertToDirectThreadItems(this List<ThreadItemModel> threadItemModels)
        {
            return threadItemModels.Select(threadItemModel => new DirectThreadItem
            {
                ItemId = threadItemModel.ItemId,
                Type = threadItemModel.ItemType,
                Timestamp = threadItemModel.Timestamp,
                Text = threadItemModel.Text ?? "",
                UserId = threadItemModel.UserId
            }).ToList();
        }

        public static long ToUnixTime(this DateTime date)
        {
            try
            {
                return Convert.ToInt64((date - UnixEpoch).TotalSeconds);
            }
            catch
            {
                return 0;
            }
        }

        public static string FirstCharToUpper(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static bool IsEqualToPattern(this string str, string pattern)
        {
            Regex rgx = new Regex(pattern);
            var matches = rgx.Matches(str);
            if (matches.Count != 1) return false;
            if (matches[0].Value.Length != str.Length) return false;

            return true;
        }
    }
}
