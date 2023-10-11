using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AutoGram.Instagram.Devices;
using AutoGram.Services;

namespace AutoGram.Instagram.Settings
{
    class Storage
    {
        private readonly Instagram _user;
        private string _destination;

        public readonly int ExperimentsRefreshInterval = 7200;
        public readonly int AppRefreshInterval = 1800;
        public readonly int EmptyCounterRefreshInterval = 21600;
        public readonly int ActivityRefreshInterval = 4000;

        public string BindDate;

        public int LastLogin;
        public int LastExperiments;

        public bool ProfileEdited;
        public bool ProfilePhotoChanged;
        public bool IsUploadedStories;
        public bool IsPostedMedia;
        public bool IsFollowedUsersOnStarting;
        public bool IsPostsDeletedAutomatically;
        public bool IsDirectSenderFeedback;

        public int SeenStories;

        public int EmptyCounter;

        public bool IsFollowSenderFinished;
        public int FollowSenderFollowingBy;

        private Stack<Activity> _activities = new Stack<Activity>();

        private Activity _activity;

        public Activity Activity
        {
            get
            {
                if (_activity != null)
                {
                    return _activity;
                }

                _activity = ActivityInitialize();

                return _activity;
            }
        }

        public Storage(Instagram user)
        {
            _user = user;

            InitializeStorage();
        }

        public void InitializeStorage()
        {
            _destination = CreateStorage(_user.Username);
        }

        public void RenameStorage(string username)
        {
            _destination = CreateStorage(username);
        }

        public static string CreateStorage(string username)
        {
            if (!Directory.Exists(Variables.FolderUsers + "/" + username))
            {
                Directory.CreateDirectory(Variables.FolderUsers + "/" + username);
            }

            return Variables.FolderUsers + "/" + username + "/" + username + ".json";
        }

        private Activity ActivityInitialize()
        {
            if (!_activities.Any())
            {
                var newActivity = new Activity(_user.GetProxy());
                _activities.Push(newActivity);

                return newActivity;
            }

            var activity = _activities.First();

            if (Utils.DateTimeNowTotalSeconds - activity.TimeStamp >= ActivityRefreshInterval)
            {
                activity = new Activity(_user.GetProxy());
                _activities.Push(activity);

                return activity;
            }

            if (activity.Actions == null
                || AutoGram.Settings.Advanced.Live.ResetCountersEveryActivity)
            {
                activity.Actions = new ActionsStats();
            }

            activity.Direct = new DirectStats();

            if (activity.Session == null)
                activity.Session = new SessionStats();

            return activity;
        }

        public bool HasUser()
        {
            return File.Exists(_destination);
        }

        public void Save()
        {
            if (!HasUser()) BindDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            Activity.Update();

            var storage = new Model.Storage
            {
                Username = _user.Username,
                Password = _user.Password,
                Biography = _user.Biography,
                BindDate = BindDate,
                AccountId = _user.AccountId,
                AdvertisingId = _user.AdvertisingId,
                ClientSessionId = _user.ClientSessionId,
                Cookies = _user.GetCookies(),
                DeviceId = _user.DeviceId,
                DeviceString = _user.Device.GetDeviceString,
                PhoneId = _user.PhoneId,
                Proxy = _user.GetProxy(),
                RankToken = _user.RankToken,
                SessionId = _user.SessionId,
                Uuid = _user.Uuid,
                UserAgent = _user.GetUserAgent(),
                ProfileEdited = ProfileEdited,
                ProfilePhotoChanged = ProfilePhotoChanged,
                EmptyCounter = EmptyCounter,
                LastLogin = Utils.DateTimeNowTotalSeconds,
                LastExpertiments = Utils.DateTimeNowTotalSeconds,
                Activities = _activities,
                App = _user.App,
                State = _user.State,
                PhoneNumber = _user.PhoneNumber,
                IsSuspended = _user.IsSuspended,
                IsAccountCreatedChallenge = _user.IsAccountCreatedChallenge,
                IsEmailChallenge = _user.IsEmailChallenge,
                IsDeletedContentChallenge = _user.IsDeletedContentChallenge,
                IsPhoneChallenge = _user.IsPhoneChallenge,
                IsUndefinedChallenge = _user.IsUndefinedChallenge,
                IsPostedMedia = IsPostedMedia,
                IsUploadedStories = IsUploadedStories,
                IsFollowedUsersOnStarting = IsFollowedUsersOnStarting,
                IsPostsDeletedAutomatically = IsPostsDeletedAutomatically,
                IsDirectSenderFeedback = IsDirectSenderFeedback,
                SeenStories = SeenStories,
                IsFollowSenderFinished = IsFollowSenderFinished,
                FollowSenderFollowingBy = FollowSenderFollowingBy,
            };

            storage.Serialize(_destination);
        }

        public void Sync()
        {
            if (!HasUser()) return;

            var storage = File.ReadAllText(_destination).Deserialize<Model.Storage>();

            _user.AccountId = storage.AccountId;
            _user.AdvertisingId = storage.AdvertisingId;
            _user.ClientSessionId = storage.ClientSessionId;
            _user.SetCookies(storage.Cookies);
            _user.DeviceId = storage.DeviceId;
            _user.App = storage.App ?? InstagramAppRepository.Get();
            _user.Device = new Device(storage.DeviceString, _user.App);
            _user.SetUserAgent(_user.Device.GetUserAgent);
            _user.PhoneId = storage.PhoneId;
            _user.PhoneNumber = storage.PhoneNumber;
            
            if (storage.State != null)
            {
                _user.State = storage.State;
            }

            if (!AutoGram.Settings.Basic.Proxy.UseProxyFromFile)
                _user.SetProxy(storage.Proxy);

            _user.RankToken = storage.RankToken;
            _user.SessionId = storage.SessionId;
            _user.Uuid = storage.Uuid;

            ProfileEdited = storage.ProfileEdited;
            ProfilePhotoChanged = storage.ProfilePhotoChanged;

            EmptyCounter = storage.EmptyCounter;

            _user.JustLoggedIn = true;
            LastLogin = storage.LastLogin;
            LastExperiments = storage.LastExpertiments;

            if (Utils.DateTimeNowTotalSeconds - LastLogin > EmptyCounterRefreshInterval)
            {
                EmptyCounter = 0;
            }

            BindDate = string.IsNullOrEmpty(storage.BindDate)
                ? Directory.GetParent(_destination).CreationTime.ToString()
                : storage.BindDate;

            _activities = storage.Activities;

            if (_activities.Count > 1)
            {
                var tempActivities = _activities.ToArray();
                _activities.Clear();
                foreach (var activity in tempActivities)
                    _activities.Push(activity);
            }

            IsPostedMedia = storage.IsPostedMedia;
            IsUploadedStories = storage.IsUploadedStories;
            IsFollowedUsersOnStarting = storage.IsFollowedUsersOnStarting;
            IsPostsDeletedAutomatically = storage.IsPostsDeletedAutomatically;
            IsDirectSenderFeedback = storage.IsDirectSenderFeedback;
            SeenStories = storage.SeenStories;
            IsFollowSenderFinished = storage.IsFollowSenderFinished;
            FollowSenderFollowingBy = storage.FollowSenderFollowingBy;
        }
    }
}
