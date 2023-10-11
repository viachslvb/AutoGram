using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Database.DirectSender;
using AutoGram.Instagram.Response.Friendship;

namespace AutoGram.Helpers
{
    internal static class UserFiltrationHelper
    {
        public static HashSet<string> WhiteList;
        public static HashSet<string> LikersWhiteList;
        public static HashSet<string> NameBlackList;

        public static List<string> AcceptedCountries;
        public static List<string> DeclinedCountries;

        // Surname Whitelist
        public static HashSet<string> SurnameWhiteList;

        // Surname Blacklist
        public static HashSet<string> SurnameBlackList;

        // Surname Hard Blacklist
        public static HashSet<string> SurnameHardBlackList;

        static UserFiltrationHelper()
        {
            AcceptedCountries =
                "🇦🇺 🇦🇹 🇧🇪 🇨🇦 🇩🇰 🇪🇺 🇫🇷 🇩🇪 🇮🇪 🇮🇹 🇯🇵 🇳🇱 🇳🇿 🇬🇧 🇸🇪 🇺🇸 🇷🇴"
                    .Split(' ').ToList();

            DeclinedCountries =
                "🇦🇫 🇦🇽 🇦🇱 🇩🇿 🇦🇸 🇦🇩 🇦🇴 🇦🇮 🇦🇶 🇦🇬 🇦🇷 🇦🇲 🇦🇼 🇦🇿 🇧🇸 🇧🇭 🇧🇩 🇧🇧 🇧🇾 🇧🇿 🇧🇯 🇧🇲 🇧🇹 🇧🇴 🇧🇦 🇧🇼 🇧🇷 🇮🇴 🇻🇬 🇧🇳 🇧🇬 🇧🇫 🇧🇮 🇰🇭 🇨🇲 🇮🇨 🇨🇻 🇧🇶 🇰🇾 🇨🇫 🇹🇩 🇨🇱 🇨🇳 🇨🇽 🇨🇨 🇨🇴 🇰🇲 🇨🇬 🇨🇩 🇨🇰 🇨🇷 🇨🇮 🇭🇷 🇨🇺 🇨🇼 🇨🇾 🇨🇿 🇩🇯 🇩🇲 🇩🇴 🇪🇨 🇪🇬 🇸🇻 🇬🇶 🇪🇷 🇪🇪 🇪🇹 🇫🇰 🇫🇴 🇫🇯 🇵🇫 🇹🇫 🇬🇦 🇬🇲 🇬🇪 🇬🇭 🇬🇮 🇬🇷 🇬🇱 🇬🇩 🇬🇵 🇬🇺 🇬🇹 🇬🇬 🇬🇳 🇬🇼 🇬🇾 🇭🇹 🇭🇳 🇭🇰 🇭🇺 🇮🇸 🇮🇳 🇮🇩 🇮🇷 🇮🇶 🇮🇲 🇮🇱 🇯🇲 🇯🇪 🇯🇴 🇰🇿 🇰🇪 🇰🇮 🇽🇰 🇰🇼 🇰🇬 🇱🇦 🇱🇻 🇱🇧 🇱🇸 🇱🇷 🇱🇾 🇱🇮 🇱🇹 🇱🇺 🇲🇴 🇲🇰 🇲🇬 🇲🇼 🇲🇾 🇲🇻 🇲🇱 🇲🇹 🇲🇭 🇲🇶 🇲🇷 🇲🇺 🇾🇹 🇲🇽 🇫🇲 🇲🇩 🇲🇨 🇲🇳 🇲🇪 🇲🇸 🇲🇦 🇲🇿 🇲🇲 🇳🇦 🇳🇷 🇳🇵 🇳🇨 🇳🇮 🇳🇪 🇳🇬 🇳🇺 🇳🇫 🇰🇵 🇲🇵 🇳🇴 🇴🇲 🇵🇰 🇵🇼 🇵🇸 🇵🇦 🇵🇬 🇵🇾 🇵🇪 🇵🇭 🇵🇳 🇵🇱 🇵🇹 🇵🇷 🇶🇦 🇷🇪 🇷🇺 🇷🇼 🇼🇸 🇸🇲 🇸🇦 🇸🇳 🇷🇸 🇸🇨 🇸🇱 🇸🇬 🇸🇽 🇸🇰 🇸🇮 🇬🇸 🇸🇧 🇸🇴 🇿🇦 🇰🇷 🇸🇸 🇪🇸 🇱🇰 🇧🇱 🇸🇭 🇰🇳 🇱🇨 🇵🇲 🇻🇨 🇸🇩 🇸🇷 🇸🇿 🇨🇭 🇸🇾 🇹🇼 🇹🇯 🇹🇿 🇹🇭 🇹🇱 🇹🇬 🇹🇰 🇹🇴 🇹🇹 🇹🇳 🇹🇷 🇹🇲 🇹🇨 🇹🇻 🇻🇮 🇺🇬 🇺🇾 🇺🇿 🇻🇺 🇻🇦 🇻🇪 🇻🇳 🇼🇫 🇪🇭 🇾🇪 🇿🇲 🇿🇼"
                    .Split(' ').ToList();

            NameBlackList = new HashSet<string>();
            foreach (var file in Directory.GetFiles("bin/Filters/NameBlacklist/"))
            {
                var names = File.ReadAllText(file)
                    .Trim()
                    .Split(' ')
                    .ToList()
                    .Where(s => s != string.Empty)
                    .Select(x => x.ToLower())
                    .Distinct()
                    .ToList();

                foreach (var name in names)
                {
                    NameBlackList.Add(name);
                }
            }


            SurnameBlackList = new HashSet<string>();
            foreach (var file in Directory.GetFiles("bin/Filters/SurnameBlacklist/"))
            {
                var names = File.ReadAllText(file)
                    .Trim()
                    .Split(' ')
                    .ToList()
                    .Where(s => s != string.Empty)
                    .Select(x => x.ToLower())
                    .Distinct()
                    .ToList();

                foreach (var name in names)
                {
                    SurnameBlackList.Add(name);
                }
            }

            SurnameWhiteList = new HashSet<string>();
            foreach (var file in Directory.GetFiles("bin/Filters/SurnameWhitelist/"))
            {
                var names = File.ReadAllText(file)
                    .Trim()
                    .Split(' ')
                    .ToList()
                    .Where(s => s != string.Empty)
                    .Select(x => x.ToLower())
                    .Distinct()
                    .ToList();

                foreach (var name in names)
                {
                    SurnameWhiteList.Add(name);
                }
            }

            WhiteList = new HashSet<string>();
            foreach (var file in Directory.GetFiles("bin/Filters/NameWhitelist/"))
            {
                var names = File.ReadAllText(file)
                    .Trim()
                    .Split(' ')
                    .ToList()
                    .Where(s => s != string.Empty)
                    .Select(x => x.ToLower())
                    .Distinct()
                    .ToList();

                foreach (var name in names)
                {
                    WhiteList.Add(name);
                }
            }

            LikersWhiteList = new HashSet<string>();
            foreach (var file in Directory.GetFiles("bin/Filters/LikersNameWhitelist/"))
            {
                var names = File.ReadAllText(file)
                    .Trim()
                    .Split(' ')
                    .ToList()
                    .Where(s => s != string.Empty)
                    .Select(x => x.ToLower())
                    .Distinct()
                    .ToList();

                foreach (var name in names)
                {
                    LikersWhiteList.Add(name);
                }
            }

            SurnameHardBlackList = new HashSet<string>();
            var nameList = File.ReadAllText("bin/Filters/surname_hard_blacklist.txt")
                .Trim()
                .Split(' ')
                .ToList()
                .Where(s => s != string.Empty)
                .Select(x => x.ToLower())
                .Distinct()
                .ToList();

            foreach (var name in nameList)
            {
                SurnameHardBlackList.Add(name);
            }
        }

        public static bool IsContainsInWhiteList(this UserDirect user, bool useSurnameWhiteLists = false, bool isMediaLikers = false)
        {
            if (!isMediaLikers)
            {
                if (!Settings.Advanced.DirectSender.Filtration.TurnOnHardFiltration)
                {
                    if (WhiteList.Where(n => n.Length > 5).Any(f => user.Username.Contains(f)))
                        return true;

                    if (WhiteList.Where(n => n.Length > 5).Any(f => user.FullName.ToLower().Contains(f)))
                        return true;
                }

                if (AcceptedCountries.Any(c => user.FullName.Contains(c)))
                    return true;

                var nameParts = user.FullName.ToLower().Split(' ');
                if (nameParts.Any(part => WhiteList.Any(x => x == part)))
                    return true;

                var fullname = user.FullName.RemoveEmojiAndConvertToUseableFormat().ToLower();

                var namePartsAnother = fullname.Split(' ');

                if (namePartsAnother.Any(part => WhiteList.Any(x => x == part)))
                    return true;

                if (useSurnameWhiteLists)
                {
                    if (namePartsAnother.Length > 1)
                    {
                        var surname = namePartsAnother[namePartsAnother.Length - 1];

                        if (surname.Length > 2)
                        {
                            if (SurnameWhiteList.Any(x => x == surname))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!Settings.Advanced.DirectSender.Filtration.TurnOnHardFiltration)
                {
                    if (LikersWhiteList.Where(n => n.Length > 5).Any(f => user.Username.Contains(f)))
                        return true;

                    if (LikersWhiteList.Where(n => n.Length > 5).Any(f => user.FullName.ToLower().Contains(f)))
                        return true;
                }

                if (AcceptedCountries.Any(c => user.FullName.Contains(c)))
                    return true;

                var nameParts = user.FullName.ToLower().Split(' ');
                if (nameParts.Any(part => LikersWhiteList.Any(x => x == part)))
                    return true;

                var fullname = user.FullName.RemoveEmojiAndConvertToUseableFormat().ToLower();

                var namePartsAnother = fullname.Split(' ');

                if (namePartsAnother.Any(part => LikersWhiteList.Any(x => x == part)))
                    return true;

                if (useSurnameWhiteLists)
                {
                    if (namePartsAnother.Length > 1)
                    {
                        var surname = namePartsAnother[namePartsAnother.Length - 1];

                        if (surname.Length > 2)
                        {
                            if (SurnameWhiteList.Any(x => x == surname))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static string RemoveEmojiAndConvertToUseableFormat(this string str)
        {
            bool isAcceptedCountry = AcceptedCountries.Any(str.Contains);
            string result;

            if (!isAcceptedCountry)
            {
                // remove emoji
                result = Regex.Replace(str,
                    @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])",
                    " ");

                // remove nonspacing mark
                string tempStr = string.Empty;
                foreach (char c in result)
                {
                    if (c != 65039) tempStr += c;
                }

                result = tempStr;
            }
            else result = str;

            // remove transparent whitespace
            // result = result.Replace("⠀", " ");

            result = result.Replace(".", " ");
            result = result.Replace("_", " ");
            //result = result.Replace("-", " ");

            // remove extra whitespaces
            result = result.Trim();
            result = Regex.Replace(result, @"\s+", " ");

            if (result.Count(Char.IsWhiteSpace) >= 6)
            {
                result = result.Replace(" ", "");
            }

            return result;
        }

        public static bool IsAccepted(this UserDirect user, Storage.Model.UserFiltration settings)
        {
            bool isAccepted = false;

            while (true)
            {
                // Filter username & fullname duplicate
                string fullname = user.FullName;
                string username = user.Username;

                if (settings.ExcludeDuplicateUsernameAndFullname)
                {
                    if (username == fullname || username == fullname.ToLower())
                        break;
                }

                // Empty profile picture: 345707102882519_2446069589734326272
                if (settings.OnlyUsersWithProfilePicture)
                {
                    if (user.ProfilePictureUrl.Contains("345707102882519_2446069589734326272"))
                        break;
                }

                // Check country
                if (DeclinedCountries.Any(c => user.FullName.Contains(c)))
                    break;

                fullname = user.FullName.RemoveEmojiAndConvertToUseableFormat().ToLower();

                if (NameBlackList.Where(n => n.Length > 5).Any(f => user.Username.Contains(f)))
                    break;

                if (settings.UseSevenCharFiltration)
                {
                    if (NameBlackList.Where(n => n.Length >= 7).Any(f => fullname.Contains(f)))
                        break;
                }

                // Filter full name
                if (settings.UsePatternEquality)
                {
                    if (!user.FullName.IsEqualToPattern(@"([a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,20}\s[a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,20})")
                        && !user.FullName.IsEqualToPattern(@"([a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,15})")
                        && !user.FullName.IsEqualToPattern(@"([a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,20}\s[a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,20}\s[a-zA-Z'ááäčďéíľňóôŕšťúýžąćęłńóśźżž]{3,20})"))
                        break;
                }

                var nameParts = fullname.Split(' ');

                if (nameParts.Any(part => NameBlackList.Any(x => x == part)))
                    break;

                // Surname Blacklist
                if (settings.UseSurnameBlackList)
                {
                    if (nameParts.Any(part => SurnameBlackList.Any(x => x == part)))
                        break;
                }

                // Surname hard blacklist
                if (settings.UseSurnameHardBlackList && nameParts.Length > 1)
                {
                    var surname = nameParts[nameParts.Length - 1];

                    if (nameParts.Count(x => x == surname) == 1 && surname.Length > 2)
                    {
                        if (SurnameHardBlackList.Any(x => x == surname))
                        {
                            break;
                        }
                    }
                }

                isAccepted = true;
                break;
            }

            return isAccepted;
        }

        public static List<UserDirect> ConvertToUserDirects(this IEnumerable<FriendshipUserModel> list)
        {
            return list
                .Select(user => new UserDirect
                {
                    Pk = user.Pk,
                    Username = user.Username,
                    FullName = user.Full_name,
                    IsPrivate = user.IsPrivate,
                    ProfilePictureUrl = user.Profile_pic_url.Replace("\u0026", "&")
                }).ToList();
        }
    }
}
