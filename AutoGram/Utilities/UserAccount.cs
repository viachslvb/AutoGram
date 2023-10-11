using System;
using System.Linq;
using System.Windows;
using Database;
using AutoGram.Services;
using xNet;

namespace AutoGram
{
    class UserAccount : IEquatable<UserAccount>
    {
        public string Username;
        public string Password;
        public Email Email;
        public string UrlProfile;

        // Additional Info
        public string DeviceString;
        public string PhoneId;
        public string Uuid;
        public string AdvertisingId;
        public string DeviceId;
        public InstagramApp App;
        public CookieDictionary Cookies;

        // Header State
        public Instagram.State HeaderState;

        public bool IsAndroidDataDefined;
        public bool IsCookiesDataDefined;

        public UserAccount()
        {
        }

        public UserAccount(string str)
        {
            try
            {
                if (str.Count(c => c == '|') > 2)
                {
                    // Account Data
                    Username = str.Split('|')[0].Split(':')[0];
                    Password = str.Split('|')[0].Split(':')[1];

                    var randomUserData = RandomUserData.Get();
                    Email = randomUserData.Email;

                    // Device Data
                    var deviceStringData = str.Split('|')[1];
                    string deviceString = string.Empty;

                    if (!string.IsNullOrEmpty(deviceStringData))
                    {
                        deviceString = Utils.TryParse(deviceStringData, @"(?<=Android.\()(.+?)(?=[0-9]{5,15})");
                        deviceString = deviceString.Replace("; ", ";");
                    }

                    DeviceString = deviceString;

                    // Android Data
                    var androidStringData = str.Split('|')[2];

                    if (!string.IsNullOrEmpty(androidStringData))
                    {
                        var androidData = androidStringData.Split(';');

                        DeviceId = androidStringData.Split(';')[0];
                        PhoneId = androidStringData.Split(';')[1];
                        Uuid = androidStringData.Split(';')[2];

                        if (androidData.Length > 3)
                            AdvertisingId = androidStringData.Split(';')[3];
                        else AdvertisingId = Utils.GenerateUUID(true);

                        IsAndroidDataDefined = true;
                    }

                    // Session Dictionary

                    var sessionData = str.Split('|')[3];
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        IsCookiesDataDefined = true;
                    }

                    sessionData = sessionData.Replace("; ", ";");
                    var sessionParams = sessionData.Split(';');

                    // Session Headers

                    HeaderState = new Instagram.State();
                    Cookies = new CookieDictionary();

                    foreach (var param in sessionParams)
                    {
                        if (param == string.Empty || param == "\"") continue;

                        var paramKey = param.Split('=')[0].ToLower();
                        if (paramKey.Contains("\"")) paramKey = paramKey.Replace("\"", "");

                        string paramValue = string.Empty;
                        if (paramKey == "authorization")
                        {
                            paramValue = param.Substring(14);
                        }
                        else paramValue = param.Split('=')[1];

                        switch (paramKey)
                        {
                            case "x-ig-www-claim":
                                HeaderState.IgWwwClaim = paramValue;
                                break;
                            case "authorization":
                                HeaderState.Authorization = paramValue;
                                break;
                            case "x-mid":
                                HeaderState.Mid = paramValue;
                                break;
                            case "ig-u-ds-user-id":
                                HeaderState.IgUserId = paramValue;
                                break;
                            case "ig-u-rur":
                                HeaderState.IgRur = paramValue;
                                break;
                            case "ig-u-ig-direct-region-hint":
                                HeaderState.IgDirectRegionHint = paramValue;
                                break;
                            default:
                                Cookies.Add(paramKey, paramValue);
                                break;
                        }
                    }

                    // Header Authorization
                    if (string.IsNullOrEmpty(HeaderState.Authorization))
                    {
                        if (Cookies.ContainsKey("sessionid")
                            && Cookies.ContainsKey("ds_user_id"))
                        {
                            string authorizationString = $"{{\"ds_user_id\":\"{Cookies["ds_user_id"]}\",\"sessionid\":\"{Cookies["sessionid"]}\",\"should_use_header_over_cookies\":true}}";

                            HeaderState.Authorization = "Bearer IGT:2:" + Utils.Base64Encode(authorizationString);
                        }
                    }

                    // Header IG-U-DS-USER-ID
                    if (string.IsNullOrEmpty(HeaderState.IgUserId))
                    {
                        if (Cookies.ContainsKey("ds_user_id"))
                        {
                            HeaderState.IgUserId = Cookies["ds_user_id"];
                            HeaderState.IgIntentedUserId = Cookies["ds_user_id"];
                        }
                    }

                    // Header IG-U-RUR
                    if (string.IsNullOrEmpty(HeaderState.IgRur))
                    {
                        if (Cookies.ContainsKey("rur"))
                        {
                            HeaderState.IgRur = Cookies["rur"];
                        }
                    }

                    if (!string.IsNullOrEmpty(HeaderState.Authorization))
                    {
                        if (Cookies.Count < 1)
                        {
                            string decodedAuthorizationToken = Utils.Base64Decode(Utils.TryParse(HeaderState.Authorization, "(?<=Bearer.IGT.2.)(.+)"));

                            if (!Cookies.ContainsKey("ds_user_id"))
                                Cookies.Add("ds_user_id", Utils.TryParse(decodedAuthorizationToken, "(?<=ds_user_id.:.)(.+?)(?=\")"));

                            if (!Cookies.ContainsKey("sessionid"))
                                Cookies.Add("sessionid", Utils.TryParse(decodedAuthorizationToken, "(?<=sessionid.:.)(.+?)(?=\")"));
                        }
                    }
                }
                else
                {
                    // Account Data
                    Username = str.Split(':')[0];
                    Password = str.Split(':')[1];

                    var randomUserData = RandomUserData.Get();
                    Email = randomUserData.Email;
                }

                App = InstagramAppRepository.Get();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Username} | {e.Message} | {e.StackTrace}");
            }
        }

        public bool Equals(UserAccount other)
        {
            return other != null && this.Username == other.Username;
        }
    }
}
