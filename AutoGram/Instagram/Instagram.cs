using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Database;
using AutoGram.Instagram.Devices;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Request;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Model;
using AutoGram.Instagram.Settings;
using AutoGram.Services;
using Newtonsoft.Json;
using xNet;
using Media = AutoGram.Instagram.Request.Media;

namespace AutoGram.Instagram
{
    class Instagram : ICloneable
    {
        public string Username;

        public string Password;

        public string FirstName;

        public string Biography;

        public Email Email;

        public string PhoneNumber;

        protected Client Client;

        public Account Account;

        public Internal Internal;

        public Device Device;

        public Timeline Timeline;

        public Media Media;

        public LiveAction LiveAction;

        public Discover Discover;

        public FriendShips FriendShips;

        public Feed Feed;

        public FbSearch FbSearch;

        public Settings.Storage Storage;

        public LiveSettings LiveSettings;

        public Activity Activity;

        public Hashtag Hashtag;

        public Direct Direct;

        public Highlights Highlights;

        public State State;

        public string AccountId;

        public string Uuid;

        public string DeviceId;

        public string PhoneId;

        public int BatteryLevel;

        public int IsCharging;

        public readonly string TimezoneOffset = "7200";

        public string AdvertisingId;

        public string RegisterWaterfallId;

        public string RankToken;

        public string SessionId;

        public string ClientSessionId;

        public string PigeonSessionId;

        public bool IsSuspended;

        public bool IsAccountCreatedChallenge;

        public bool IsPhoneChallenge;

        public bool IsEmailChallenge;

        public bool IsDeletedContentChallenge;

        public bool IsUndefinedChallenge;

        public bool JustLoggedIn;
        public Request.Request Request => Client.Request;

        public InstagramApp App;

        public bool IsAuthorizedDevice;

        public string ProfileUrl;

        public bool IsProfileUrl;

        public Worker Worker;

        public DirectRepository DirectRepository;

        public bool DisableStorageSaving;

        public Instagram(string username, string password, Email email, RandomUserData userData = null, AndroidDevice androidDevice = null, Worker worker = null, UserAccount userAccount = null)
        {
            Username = username;
            Password = password;
            Email = email;

            if (userData != null)
            {
                FirstName = userData.FirstName;
            }

            if (worker != null)
            {
                Worker = worker;
            }

            // Load all function collections.
            Client = new Client();
            LiveAction = new LiveAction(this);
            Internal = new Internal(this);
            Timeline = new Timeline(this);
            Account = new Account(this);
            Media = new Media(this);
            Storage = new Settings.Storage(this);
            Discover = new Discover(this);
            FriendShips = new FriendShips(this);
            Feed = new Feed(this);
            Hashtag = new Hashtag(this);
            FbSearch = new FbSearch(this);
            LiveSettings = new LiveSettings();
            Highlights = new Highlights(this);
            Direct = new Direct(this);
            State = new State();

            // Storage sync
            if (Storage.HasUser())
            {
                Storage.Sync();

                if (App == null)
                {
                    App = InstagramAppRepository.Get();
                }

                IsAuthorizedDevice = true;
            }
            else
                InitUser(userAccount);

            if (AutoGram.Settings.Advanced.General.ForceChangingInstagramApp)
            {
                App = InstagramAppRepository.Get();
                Device = new Device(App);
                SetUserAgent(Device.GetUserAgent);
            }

            Request.SetApp(App);
            Request.SetDevice(Device);
            Request.SetUser(this);

            RegisterWaterfallId = Utils.GenerateUUID(true);
            PigeonSessionId = Utils.GeneratePigeonSession();

            BatteryLevel = Utils.Random.Next(50, 99);
            IsCharging = Utils.UseIt() ? 1 : 0;
        }

        public void SetProxy(Proxy proxy) => Client.SetProxy(proxy);

        public Proxy GetProxy() => Client.GetProxy();

        public bool IsProxy() => Client.GetProxy() != null;

        public void RemoveProxy() => Client.RemoveProxy();

        public CookieDictionary GetCookies() => Client.GetCookies();

        public string GetCookieValue(string cookieKey) => Client.GetCookieValue(cookieKey);

        public CookieDictionary CloneCookies() => Client.CloneCookies();

        public void SetCookies(CookieDictionary cookies) => Client.SetCookies(cookies);

        public void RemoveCookieByKey(string key) => Client.RemoveCookieByKey(key);

        public void ClearCookies() => Client.ClearCookies();

        public string GetToken() => Client.GetToken();

        public string GetUserIdFromSession() => Client.GetUserId();
        public string GetSessionIdFromSession() => Client.GetSessionId();

        public void SetUserAgent(string userAgent) => Client.SetUserAgent(userAgent);

        public string GetUserAgent() => Device.GetUserAgent;

        public void SetUrlProfile(string urlProfile) => ProfileUrl = urlProfile;

        public bool HasPhoneNumber() => !string.IsNullOrEmpty(PhoneNumber);

        public UserResponse UserInfo;

        private void InitUser(UserAccount userAccount)
        {
            if (string.IsNullOrEmpty(userAccount.DeviceString))
            {
                App = InstagramAppRepository.Get();
                Device = new Device(App);
            }
            else
            {
                App = userAccount.App;
                Device = new Device(userAccount.DeviceString, App);
            }

            if (userAccount.IsCookiesDataDefined)
            {
                SetCookies(userAccount.Cookies);
                State = userAccount.HeaderState;
                JustLoggedIn = true;
            }

            if (userAccount.IsAndroidDataDefined)
            {
                Uuid = userAccount.Uuid;
                DeviceId = userAccount.DeviceId;
                PhoneId = userAccount.PhoneId;
                AdvertisingId = userAccount.AdvertisingId;
            }
            else
            {
                Uuid = Utils.GenerateUUID(true);
                DeviceId = Utils.GenerateDeviceID(Username + Password);
                PhoneId = Utils.GenerateUUID(true);
                AdvertisingId = Utils.GenerateUUID(true);
            }

            SessionId = Utils.GenerateUUID(true);
            ClientSessionId = Utils.GenerateUUID(true);
            SetUserAgent(Device.GetUserAgent);

            string accountId = string.Empty;
            if (userAccount.IsCookiesDataDefined)
            {
                if (userAccount.Cookies.ContainsKey("ds_user_id"))
                    accountId = userAccount.Cookies["ds_user_id"];
                else if (!string.IsNullOrEmpty(State.IgUserId))
                    accountId = State.IgUserId;
            }

            if (!string.IsNullOrEmpty(accountId))
            {
                AccountId = accountId;
                RankToken = AccountId + "_" + Uuid;
            }
        }

        public LoginResponse Login(bool forceLogin = false)
        {
            try
            {
                if (!JustLoggedIn || forceLogin)
                {
                    ClearCookies();
                    SendPreLoginFlow();

                    var response = new LoginResponse();
                    try
                    {
                        response = Do(() => Account.Login());

                        if (string.IsNullOrEmpty(State.Authorization))
                        {
                            string dsUserId = GetUserIdFromSession();
                            string sessionId = GetSessionIdFromSession();

                            if (dsUserId != string.Empty && sessionId != string.Empty)
                            {
                                string authorizationString = $"{{\"ds_user_id\":\"{dsUserId}\",\"sessionid\":\"{sessionId}\"}}";
                                State.Authorization = "Bearer IGT:2:" + Utils.Base64Encode(authorizationString);
                            }
                        }

                        SaveStorage();
                    }
                    catch (DeletedUserException ex)
                    {
                        Log("Account was deleted. Restoring account...");
                        response = Do(() => Account.RestoreAccountAndLogin(ex.Message));
                    }

                    if (!response.IsOk())
                        return response;

                    AccountId = response.User.Pk;
                    RankToken = AccountId + "_" + Uuid;

                    SendLoginFlow();

                    return response;
                }
            }
            catch (LoginRequiredException)
            {
                return Login(true);
            }

            try
            {
                if (string.IsNullOrEmpty(AccountId))
                {
                    AccountId = GetUserIdFromSession();
                    RankToken = AccountId + "_" + Uuid;
                }

                SendLoginFlow(true);

                if (string.IsNullOrEmpty(AccountId))
                {
                    AccountId = GetUserIdFromSession();
                    RankToken = AccountId + "_" + Uuid;
                }

                return new LoginResponse { Status = "ok" };
            }
            catch (LoginRequiredException)
            {
                return Login(true);
            }
        }

        public void SendPreLoginFlow()
        {
            bool minimizeRequests = AutoGram.Settings.Advanced.General.MinimizeInstagramRequests;

            if (!minimizeRequests)
            {
                Do(() => Internal.SyncDeviceFeatures(true));
                Do(() => Internal.PreLoginLauncherSync());
                Do(() => Internal.PreLoginLauncherSync());
                Do(() => Internal.LogAttribution());
                Do(() => Internal.PreLoginLauncherSync(true));
                Do(() => Internal.PreLoginLauncherSync(true));
                Do(() => Internal.SyncDeviceFeatures(true, true));

                Utils.RandomSleep(4000, 6000);
            }
        }

        public void SendLoginFlow(bool justLoggedIn = false, FeedTimelineResponse loginResponse = null)
        {
            bool minimizeRequests = AutoGram.Settings.Advanced.General.MinimizeInstagramRequests;

            bool isSessionExpired = Storage.LastLogin == 0 || Utils.DateTimeNowTotalSeconds - Storage.LastLogin > Storage.AppRefreshInterval;
            bool isExperimentExpired = Storage.LastExperiments == 0 || Utils.DateTimeNowTotalSeconds - Storage.LastExperiments > Storage.ExperimentsRefreshInterval;

            if (!justLoggedIn) // After Login
            {
                // Set nav-chain
                State.IgNavChain = "1nj:feed_timeline:1";

                if (!minimizeRequests)
                {
                    Do(() => Internal.PostLoginLauncherSync());
                    Do(() => Account.GetAccountFamily());
                    Do(() => Internal.AsyncGetNdxIgSteps());
                    Do(() => Timeline.GetTimelineFeed(feedViewInfoEnable: true, reason: "cold_start_fetch",
                            unseenPostsEnable: false));
                    Do(() => Internal.ReelsTray("cold_start"));
                    Do(() => Internal.FetchZeroRatingToken());
                    Do(() => Internal.GetSharePrefill("[\"story_share_sheet\",\"direct_user_search_nullstate\",\"forwarding_recipient_sheet\",\"threads_people_picker\",\"direct_inbox_active_now\",\"group_stories_share_sheet\",\"call_recipients\",\"reshare_share_sheet\",\"direct_user_search_keypressed\"]"));
                    Do(() => Internal.GetNotificationBadge());
                    Do(() => Internal.GetNotificationBadge());
                    Do(() => Internal.GetNotificationBadge());
                    Do(() => Internal.NewsInbox());
                    Do(() => Internal.ScoresBootstrap());
                    Do(() => Internal.GetCooldowns());
                    Do(() => Internal.MediaBlocked());
                    //Do(() => Internal.InjectedReelsMedia()); //todo: fix
                    Do(() => Internal.AccountLinking());
                    Do(() => Timeline.GetTimelineFeed(reason: "cold_start_fetch",
                                recoveryFromCrash: true, unseenPostsEnable: false));
                    Do(() => Discover.TopicalExplore());
                    Do(() => Internal.DirectInbox(limit: "20", threadMessageLimit: "10", fetchReason: "initial_snapshot"));
                    Do(() => Internal.DirectGetPresence());
                    Do(() => Internal.GetViewableStatuses());
                    Do(() => Account.GetCurrentUser());
                    Do(() => Internal.GraphQl(friendlyName: "IGFxLinkedAccountsQuery", docId: "4324170747611977", "{}"));
                    Do(() => Internal.GraphQl(friendlyName: "SessionSurveyUriQuery", docId: "3789388284511218", $"{{\"integration_point_id\":\"449092836056930\",\"session_id\":\"{PigeonSessionId}\"}}"));
                    Do(() => Internal.QpBatchFetch());
                    Do(() => Internal.DirectInbox(limit: "0"));
                    Do(() => Internal.GetNotificationBadge());
                    Do(() => Internal.GraphQl(friendlyName: "IGPaymentsAccountDisabledRiskQuery", docId: "2897674770271335", "{}"));
                    Do(() => Internal.QpBatchFetch());
                }
            }
            else
            {
                List<Action> actions;

                if (isSessionExpired)
                {
                    Do(() => Timeline.GetTimelineFeed(feedViewInfoEnable: true, reason: "cold_start_fetch",
                        unseenPostsEnable: false));
                    Do(() => Internal.ReelsTray("cold_start"));

                    if (!minimizeRequests)
                    {
                        Do(() => Internal.GetNotificationBadge());
                        Do(() => Internal.PostLoginLauncherSync());
                        Do(() => Internal.PostLoginLauncherSync(idIsUuid: true));
                        //Do(() => Internal.InjectedReelsMedia()); //todo: fix
                        Do(() => Timeline.GetTimelineFeed(feedViewInfoEnable: true, reason: "cold_start_fetch",
                            unseenPostsEnable: false));
                        Do(() => Internal.GraphQl(friendlyName: "IGPaymentsAccountDisabledRiskQuery", docId: "2897674770271335", "{}"));
                        Do(() => Feed.GetUserFeedStory());
                        Do(() => Internal.QpBatchFetch());
                        Do(() => Internal.GraphQl(friendlyName: "IgDonationsEligibilityQuery", docId: "2615360401861024", "{}"));
                        Do(() => Internal.GraphQl(friendlyName: "IGFBPayExperienceEnabled", docId: "3801135729903457", "{}"));
                        Do(() => Internal.GraphQl(friendlyName: "IgPaymentsSettingsInfoQuery", docId: "3074914985892821", "{\"payment_type\":\"ig_payment_settings\"}"));
                        Do(() => Internal.HighLightsTray());
                        Do(() => Feed.GetUserFeed());
                        Do(() => Account.GetUserInfo(fromModule: "self_profile"));
                        Do(() => Internal.ProfileArchiveBadge());
                        Do(() => Internal.FbGetInviteSuggestions());
                        Do(() => Internal.FbGetInviteSuggestions(first: false));
                        Do(() => Internal.FundRaiser());
                        Do(() => Discover.GetSuggestedUsers(module: "self_profile"));
                        Do(() => Internal.GetSharePrefill("[\"threads_people_picker\"]"));
                        UserInfo = Do(() => Account.GetUserInfo());
                        Do(() => Internal.ArlinkDownloadInfo());
                        Do(() => Internal.HasInteropUpgraded());
                        Do(() => Internal.GetViewableStatuses());
                        Do(() => Account.GetAccountFamily());
                        Do(() => Internal.DirectInbox(limit: "0", threadMessageLimit: "10"));
                        Do(() => Internal.DirectGetPresence());
                        Do(() => Internal.GraphQl(friendlyName: "IGFxLinkedAccountsQuery", docId: "4324170747611977", "{}"));
                        Do(() => Internal.GetSharePrefill("[\"story_share_sheet\",\"direct_user_search_nullstate\",\"forwarding_recipient_sheet\",\"threads_people_picker\",\"direct_inbox_active_now\",\"group_stories_share_sheet\",\"call_recipients\",\"reshare_share_sheet\",\"direct_user_search_keypressed\"]"));
                        Do(() => Internal.GraphQl(friendlyName: "SessionSurveyUriQuery", docId: "3789388284511218", $"{{\"integration_point_id\":\"449092836056930\",\"session_id\":\"{PigeonSessionId}\"}}"));
                        Do(() => Internal.GetNotificationBadge());
                        Do(() => Internal.SyncUserFeatures());
                        Do(() => Internal.SyncDeviceFeatures());
                        Do(() => Internal.FetchZeroRatingToken(reason: "token_stale"));
                    }

                    if (string.IsNullOrEmpty(AccountId))
                    {
                        AccountId = GetUserIdFromSession();
                        RankToken = AccountId + "_" + Uuid;
                    }
                }
                else
                {
                    if (isExperimentExpired)
                    {
                        actions = new List<Action>
                        {
                            () => Do(() => Internal.SyncDeviceFeatures(true, true)),
                            () => Do(() => Internal.PreLoginLauncherSync(true)),
                            () => Do(() => Internal.SyncUserFeatures()),
                            () => Do(() => Internal.PostLoginLauncherSync()),
                        };
                        actions.Shuffle();
                        actions.ForEach(a => a());
                    }

                    actions = new List<Action>
                    {
                        () => Do(() => Timeline.GetTimelineFeed(reason: "cold_start_fetch",
                            recoveryFromCrash: true, unseenPostsEnable: false)),
                        () => Do(() => Internal.ReelsTray())
                    };
                    actions.Shuffle();
                    actions.ForEach(a => a());
                }
            }

            Activity = Storage.Activity;
            Storage.Save();
        }

        public void SaveStorage()
        {
            Activity = Storage.Activity;
            Storage.Save();
        }

        public void Log(string message)
        {
            Worker.Account.WriteLog(message);
        }

        public void Do(Action action)
        {
            Do(() => { action(); return 0; });
        }

        public T Do<T>(Func<T> action, bool ignoreJsonErrors = false)
        {
            int errorsCount = 0;

            while (true)
            {
                try
                {
                    T response = action();

                    // Update HTTP headers
                    State.Update(response);

                    if (response is TraitResponse traitResponse)
                    {
                        // Is Ok
                        if (traitResponse.IsOk()) return response;

                        // Instagram exceptions handler

                        if (traitResponse.IsConsentRequired())
                        {
                            var consentResponse = Do(() => Internal.ConsentExistingUserFlow());
                            Log($"Consent required: {consentResponse.ScreenKey}");

                            if (consentResponse.IsDobConsent)
                            {
                                consentResponse = Do(() => Internal.ConsentDob());

                                if (consentResponse.IsFinished)
                                    Log($"Consent finished.");
                            }
                            else if (consentResponse.IsQpIntro)
                            {
                                consentResponse = Do(() => Internal.ConsentQpIntro());

                                if (consentResponse.IsFinished)
                                    Log($"Consent finished.");
                            }
                        }

                        if (traitResponse.IsChallengeRequired())
                        {
                            if (traitResponse.IsPrivacyFlow())
                            {
                                var privacyFlowResponse = Do(() => Internal.PrivacyAccept());
                            }
                        }

                        if (traitResponse.IsChallenge())
                        {
                            var challenge = new Request.Challenge(this, traitResponse.Challenge);
                            var challengeType = challenge.Type;

                            if (challengeType.IsPhoneChallenge())
                            {
                                bool challengeConfirmed = false;

                                if (AutoGram.Settings.Advanced.General.VerifyPhoneWhenChallenge
                                    && string.IsNullOrEmpty(this.PhoneNumber))
                                {
                                    int attemptionCounter = 0;

                                    if (string.IsNullOrEmpty(challengeType.UserId)
                                        || string.IsNullOrEmpty(challengeType.NonceCode))
                                    {
                                        var loginResponse = Do(() => Login(true));

                                        if (loginResponse.IsOk())
                                        {
                                            continue;
                                        }
                                    }

                                    while (true)
                                    {
                                        try
                                        {
                                            this.Log("Phone verification challenge.");

                                            // Phone verification
                                            IPhoneVerificationService phoneVerificationService = new SmsBoostService();

                                            string phoneNumber = phoneVerificationService.GetPhoneNumber();
                                            this.Log($"Verification phone number: {phoneNumber}");

                                            var sendCodeResponse =
                                                Do(
                                                    () =>
                                                        challenge.SendPhoneVerificationCode(challengeType.UserId,
                                                            challengeType.NonceCode, phoneNumber));

                                            string errorMessage;
                                            if (sendCodeResponse.IsOk())
                                            {
                                                this.Log($"Sms code was sent to {phoneNumber}.");
                                                Thread.Sleep(10000);

                                                string verificationCode =
                                                    phoneVerificationService.ReceiveVerificationCode(phoneNumber);

                                                var verifyCodeResponse =
                                                    Do(
                                                        () =>
                                                            challenge.VerifyPhoneVerificationCode(challengeType.UserId,
                                                                challengeType.NonceCode, verificationCode));

                                                if (verifyCodeResponse.IsOk())
                                                {
                                                    this.Log($"Phone number was successfully validated.");
                                                    this.PhoneNumber = phoneNumber;
                                                    challengeConfirmed = true;

                                                    break;
                                                }

                                                errorMessage = verifyCodeResponse.IsMessage()
                                                    ? verifyCodeResponse.GetMessage()
                                                    : "Function [validateCodeResponse] does not return [Okay]";
                                                AutoGram.Log.Write(errorMessage);
                                                Log(errorMessage);
                                            }
                                            else
                                            {
                                                errorMessage = sendCodeResponse.IsMessage()
                                                    ? sendCodeResponse.GetMessage()
                                                    : "Function [sendCodeResponse] does not return [Okay]";
                                                AutoGram.Log.Write(errorMessage);
                                                Log(errorMessage);
                                            }
                                        }
                                        catch (VerificationCodeWaitingTimeoutException)
                                        {
                                            this.Log("Timeout waiting for verification code.");
                                        }
                                        catch (VerificationServiceErrorLimitException)
                                        {
                                            this.Log("Verification service error limit exception.");
                                        }
                                        catch (VerificationServiceZeroBalanceException)
                                        {
                                            this.Log("Verification service zero balance exception.");
                                        }
                                        catch (VerificationServiceFailedException)
                                        {
                                            this.Log("Verification service failed exception.");
                                        }

                                        if (attemptionCounter >= 3) break;

                                        challenge.ResetVerificationPhone(challengeType.UserId, challengeType.NonceCode);
                                        attemptionCounter++;
                                    }

                                    if (!challengeConfirmed)
                                        throw new PhoneVerificationChallengeException();
                                    continue;
                                }

                                throw new PhoneVerificationChallengeException();
                            }

                            if (challengeType.IsEmailChallenge())
                            {
                                throw new EmailVerificationChallengeException();
                            }

                            if (challengeType.IsDeletedContentChallenge())
                            {
                                throw new DeletedContentChallengeException();
                            }

                            if (challengeType.IsUndefinedChallenge())
                            {
                                throw new ChallengeRequiredException();
                            }

                            if (challengeType.IsAutomatedBehavior())
                            {
                                this.Log("Automated Behavior Challenge");
                                var abChallengeResponse = challenge.CompleteAutomatedBehaviorChallenge(challengeType.CniValue);
                                if (abChallengeResponse.IsOk())
                                {
                                    this.Log("Automated Behavior Challenge Completed");
                                }
                            }
                        }

                        if (traitResponse.IsLoginRequired())
                        {
                            var loginResponse = Do(() => Login(true));

                            if (loginResponse.IsOk())
                            {
                                continue;
                            }
                            else
                            {
                                throw new LoginRequiredException();
                            }
                        }

                        if (traitResponse.IsDeletedUser())
                        {
                            throw new DeletedUserException(traitResponse.GetStopDeletionToken());
                        }

                        if (traitResponse.IsInactiveUser())
                        {
                            throw new InactiveUserException();
                        }
                    }

                    return response;
                }
                catch (HttpException ex)
                {
                    Proxy.CheckConnection(this, ref errorsCount);
                }
                catch (JsonReaderException exception)
                {
                    if (ignoreJsonErrors)
                        throw new IgnoreJsonErrorsException();

                    var errorMessage = "JsonReaderException";

                    if (errorsCount == 3)
                        Log($"{errorMessage}. Try again.");

                    if (errorsCount >= 10)
                    {
                        AutoGram.Log.Write($"{errorMessage} [{exception.Message}] | Stacktrace: {exception.StackTrace} [{Username}]", LogResource.JsonReader);
                        throw new SomethingWrongException($"{errorMessage}. Failed.");
                    }

                    errorsCount++;

                    Thread.Sleep(5000);
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}