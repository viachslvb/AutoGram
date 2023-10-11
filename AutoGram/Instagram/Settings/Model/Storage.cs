using System.Collections.Generic;
using Database;
using Newtonsoft.Json;
using xNet;

namespace AutoGram.Instagram.Settings.Model
{
    class Storage
    {
        public string Username;

        public string Password;

        [JsonProperty("Binded at")]
        public string BindDate;

        public Proxy Proxy;

        public string PhoneNumber;

        public string Biography;

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

        public bool ProfileEdited;

        public bool ProfilePhotoChanged;

        public int EmptyCounter;

        public int LastLogin;

        public int LastExpertiments;

        [JsonProperty("Log Activity")]
        public Stack<Activity> Activities = new Stack<Activity>();

        public InstagramApp App;

        public State State;

        public bool CheckedFbFriends;

        public bool IsSuspended;

        public bool IsAccountCreatedChallenge;

        public bool IsPhoneChallenge;

        public bool IsEmailChallenge;

        public bool IsDeletedContentChallenge;

        public bool IsUndefinedChallenge;

        public bool IsDirectSenderFeedback;

        public bool IsPostsDeletedAutomatically;

        public bool IsPostedMedia;

        public bool IsUploadedStories;

        public bool IsFollowedUsersOnStarting;

        public int SeenStories;

        public bool IsFollowSenderFinished;

        public int FollowSenderFollowingBy;
    }
}
