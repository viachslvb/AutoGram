using System.Windows.Documents;

namespace AutoGram.Storage.Model
{
    class AdvancedSettings
    {
        public bool VerifyEmailViaProxy;
        public EmailVerificationProxy EmailVerificationProxy;
        public General General;
        public ProfileAdvanced Profile;
        public FollowSender FollowSender;
        public CreatorChildAccounts CreatorChildAccounts;
        public ImageAdvanced Image;
        public PostAdvanced Post;
        public CommentAdvanced Comment;
        public DirectAdvanced Direct;
        public StoryViewer StoryViewer;
        public DirectSender DirectSender;
        public Notifier Notifier;
        public ProfileChecker ProfileChecker;
        public FollowersParser FollowersParser;
        public PostAfterRegistration PostAfterRegistration;
        public RegisterAdvanced Register;
        public LiveMode Live;
        public HashtagAdvanced Hashtag;
        public WorkersSettings WorkersSettings;
        public bool RandomizeInstagramApps;
    }

    class General
    {
        public bool VerifyPhoneWhenChallenge;
        public bool NotificationNightMode;
        public bool IsEconomyMode;
        public bool MinimizeInstagramRequests;
        public bool SyncUiWithInstagramAccount;
        public bool IsAccountsLoopingMode;
        public bool UseCustomUserFormatter;
        public bool UseInstaAccountsManagerFormat;
        public bool UseCookiesOnlyFormat;
        public bool UseCookiesOnlyFormatJustLogged;
        public bool UseInstaAccountsManagerFormatWithoutCookies;
        public bool ForceChangingInstagramApp;
        public int AccountsLoops;
        public int AccountsPerSession;
        public int AccountLoopsPerSession;
        public bool UsePauseBetweenAccounts;
        public int PauseFrom;
        public int PauseTo;
    }

    class DirectAdvanced
    {
        public bool Enable;
        public bool CheckInboxOnly;
        public bool WatchVisualThreadItems;
        public bool SendDisappearingPhoto;
        public bool LikesMessageThatSendingAfterPhoto;
        public bool SendWowMessageInResponseToMedia;
        public string DirectUrl;
        public int RefreshThreadDelay;
        public int RefreshThreadCount;
        public int RefreshThreadsDelay;
        public int RefreshThreadsCount;
        public bool IsShortAnswer;
        public int ShortAnswerType;
        public bool MaxUniqueizePhoto;
    }

    class FollowersParser
    {
        public bool Enable;
        public string UsernameSource;
        public bool ParseFollowings;
        public int MaximumFromEach;
        public int PauseMilliseconds;
    }

    class ProfileChecker
    {
        public bool Enable;
        public string ProfileList;
    }

    class DirectSender
    {
        public bool Enable;
        public bool ParseOnlyMode;
        public bool UseDatabase;
        public string UsernameSourceMarker;
        public string UsernameSource;
        public bool UseLikers;
        public bool UseLikersWhenFollowersNotEnough;
        public bool MuteLevelsIsEnabled;
        public int DeepParsing;
        public UserFiltration Filtration;
        public bool SendOneMessage;
        public int Count;
        public bool UpdateTitle;
        public string Title;
        public string Domains;
        public bool UseNameFromAccount;
        public bool SendMessageAsText;
        public string MessageText;
        public string[] Messages;
        public bool SendAdditionalMessageText;
        public string AdditionalMessageText;
        public bool UseRandomGreetings;
        public bool AddAdditionalTemplates;
        public bool AddUsersToConversation;
        public bool AddAllToConversation;
        public int ParticipantsCount;
        public int PauseMilliseconds;
        public DirectSenderCustom CustomUrl;
    }

    class FollowSender
    {
        public bool Enable;
        public string UserProfilesSourceLabel;
        public string UserProfilesSource;
        public bool MuteLevelsIsEnabled;
        public UserFiltration Filtration;
        public FollowSenderSettings FollowSettings;
    }

    class FollowSenderSettings
    {
        public int FollowFromEachAccountLimit;
        public int FollowPerActionLimit;
        public int SearchAndFollowLoopLimit;
        public int PauseBetweenFollowing;
        public int PauseBetweenLoopInSeconds;
    }

    class Notifier
    {
        public bool Enable;
        public string UsernameSource;
        public bool UseLikers;
        public bool UseLikersWhenFollowersNotEnough;
        public UserFiltration Filtration;
        public int CountParticipants;
        public string DirectUrl;

    }

    class DirectSenderCustom
    {
        public bool Enable;
        public string Url;
        public string Title;
        public string Greetings;
        public bool UseDirectUserData;
    }

    class CreatorChildAccounts
    {
        public bool Enable;
        public int FailsLimit;
        public bool UsePostRegistrationFlow;
        public bool UseLightVersionForEditingProfile;
        public bool UploadProfilePicture;
        public bool ShareToFeedProfilePicture;
        public bool FillFullnameDirectly;
        public bool SetPrivateProfile;
        public bool EditProfile;
    }

    class UserFiltration
    {
        public bool Enable;
        public bool TurnOnHardFiltration;
        public bool UseWhiteList;
        public bool UseReverseWhiteList;
        public bool UseSurnameWhiteList;
        public bool UseSurnameBlackList;
        public bool UseSurnameHardBlackList;
        public bool ExcludeDuplicateUsernameAndFullname;
        public bool UseSevenCharFiltration;
        public bool UseMexicanNames;
        public bool UsePatternEquality;
        public bool OnlyUsersWithProfilePicture;
    }

    class DirectSenderSearchResultsChecker
    {
        public bool Enable;
        public string TargetUserPk;
        public string ExpectedUsernames;
        public bool UseTechAccounts;
    }

    class StoryViewer
    {
        public bool Enable;
        public string UsernameSource;
        public bool UseLikersForViewingStories;
        public int Count;
        public int PauseMilliseconds;
    }

    class LiveMode
    {
        public bool Enable;
        public bool ResetCountersEveryActivity;
        public SuggestedFriendsSettings SuggestedFriends;
        public FollowUsersSettings FollowUsersOnStarting;
        public FollowSettings FollowSettings;
        public LikeSettings LikeSettings;
        public ExploreProfiles ExploreProfilesSettings;
    }

    class FollowUsersSettings
    {
        public bool Use;
        public string Users;
    }

    class SuggestedFriendsSettings
    {
        public bool Use;
        public bool UseExploringProfilesWithChaining;
        public bool UseAllowUsersList;
        public string AllowUsersOnStarting;
        public bool FollowAllFriendsSuggestions;
    }

    class ExploreProfiles
    {
        public int ViewingDepthLimit;
        public int Limit;
        public RandomLimit RandomLimit;
    }

    class ExplorePostSettings
    {
        public int Depth;
        public bool OpenUserProfilesFromComments;
    }

    class ExploreSettings
    {
        public bool OpenChainingFriends;
        public int ChainingFriendsDeepLimit;
    }

    class LikeSettings
    {
        public int Limit;
        public RandomLimit RandomLimit;
    }

    class FollowSettings
    {
        public Delay Delay;
        public int Limit;
        public RandomLimit RandomLimit;
        public bool IsVerifiedMoreImportant;
    }

    class EmailVerificationProxy
    {
        public string Ip;
        public int Port;
        public string Username;
        public string Password;
    }

    class ProfileAdvanced
    {
        public bool IsPrivateProfile;
        public StoriesSettings Stories;
        public bool ChangeUsernameToDefault;
        public bool ChangeProfileUrl;
        public bool ChangeProfileName;
        public bool ChangeBiography;
        public bool UseAdvancedDescriptions;
        public string[] ProfileDescriptions;
        public string[] PhotoDescriptions;
        public string UrlIndentificator;
    }

    class StoriesSettings
    {
        public bool UploadStories;
        public bool AddToHighlights;
        public string HighlightTitle;
    }

    class RegisterAdvanced
    {
        public RegisterViaPhoneNumber RegisterViaPhoneNumber;
        public RegisterDevice Device;
        public RegistrationLimits Limits;
        public bool UploadProfilePicture;
        public bool ContinueAfterRegistration;
        public bool RandomName;
        public string RegistrationName;
        public bool RandomizeEmails;
        public bool RandomizeUsernames;
        public bool UseRealUsernames;
        public bool ReduceDelay;
        public bool SkipSignupFlow;
        public bool SkipHomeWalking;
    }

    class RegisterDevice
    {
        public bool UseAndroidDeviceDatabase;
        public bool UseDeviceDataDatabase;
        public bool UseDeviceDataFile;
    }

    class RegistrationLimits
    {
        public int Accounts;
        public int GlobalFailed;
        public int LocalFailed;
        public int MaximumErrorsChain;
    }

    class RegisterViaPhoneNumber
    {
        public bool Enable;
        public bool Manual;
        public bool UseSmsBoostService;
    }

    class ImageAdvanced
    {
        public bool EnableImaginaryPlus;
        public bool HighImagine;
        public bool HighQuality;
        public bool UseLiquidRescale;
        public int LiquidRescalePercentage;
        public RandomLimit Noise;
        public Blur Blur;
        public ImageText Text;
        public ImageLines Lines;
        public bool AddKikProfile;
        public ProfilePicture ProfilePicture;
    }

    class ProfilePicture
    {
        public Blur Blur;
        public ImageLines Lines;
    }

    class Blur
    {
        public bool Use;
        public Delay Radius;
        public Delay Sigma;
    }

    class ImageLines
    {
        public bool Draw;
        public bool UseImaginary;
        public bool IsBackground;
        public bool UseThickLines;
        public bool UseThinLines;
        public Delay Opacity;
        public bool DrawTopRightSide;
        public bool DrawLeftBottomSide;
        public bool UseColorfulLine;
    }

    class ImageText
    {
        public bool Draw;
        public string Title;
        public bool UseImaginary;
        public bool Swirl;
        public bool LargeSize;
        public bool Colorize;
        public bool UseDifferentFonts;
        public bool RandomPosition;
        public bool RandomTrancparency;
    }

    class PostAdvanced
    {
        public bool Enable;
        public bool RequireOnce;
        public bool CheckDirectAfterPosting;
        public int WaitDirect;
        public bool DontPublishIfPostsDeletedAutomatically;
        public bool UsePostsDatabase;
        public bool UsePostsDatabaseCaptionsOnly;
        public bool SuspentIfPostAutoDeleted;
        public PostType Type;
        public RandomLimit RandomLimit;
        public PostContent Content;
    }

    class CommentAdvanced
    {
        public bool Enable;
        public int TaskCountPerSession;
        public bool CheckDirectAfterCommenting;
        public int WaitDirect;
        public string TagSource;
        public string CommentSource;
        public bool LoadPopularFeed;
        public bool LoadRecentFeed;
        public int TakeMediaCount;
        public bool ShuffleMedia;
        public bool UseCleverSniffer;
        public PreviewCommentsFilter PreviewCommentsFilter;
        public RandomLimit MediaWhereCommentCount;
        public RandomLimit MediaWhereLikeCount;
        public int MediaOnlyForTheLastDays;
        public RecentThreeComments RecentThreeCommentsFilter;
        public int DontRepeatIfCommentedAlreadyInLast;
        public RandomLimit QuantityLimit;
        public Delay Delay;
    }

    class RecentThreeComments
    {
        public bool Enable;
        public int MinutesLimit;
    }

    class PreviewCommentsFilter
    {
        public bool Enable;
        public int MinutesLimit;
        public bool DontRepeatIfCommentedAlready;
        public bool OrderByCreatedDate;
    }

    class PostContent
    {
        public bool UseAdvancedSettings;
        public ContentCaption Caption;
        public bool AddComment;
        public ContentComment Comment;
    }

    class ContentCaption
    {
        public bool WithTargetCaption;
        public bool UseSeparator;
        public int SeparatorsLimit;
        public HashtagAdvanced SeparatorsRandomLimit;
        public bool AddTags;
        public bool TagsWithRandomText;
        public int TagsLimit;
        public HashtagAdvanced TagsRandomLimit;
        public bool OnlyRandomText;
        public bool EmptyCaption;
    }

    class ContentComment
    {
        public bool TagsWithRandomText;
        public bool HideComment;
    }

    class Delay
    {
        public int From;
        public int To;
    }

    class PostAfterRegistration
    {
        public bool Use;
        public int From;
        public int To;
        public bool IsOnlyRandomTextCaption;
        public bool EditProfile;
        public bool EditProfileRandom;
        public Delay Delay;
    }

    class RandomLimit
    {
        public bool Use;
        public int From;
        public int To;
    }

    class PostType
    {
        public bool RandomizePostingType;
        public bool EnableRandomizingVideoType;
        public bool SlidePosting;
        public bool DifferentSlidePhotos;
        public bool UseVideoFromFolder;
        public bool UseVideoKik;
    }

    class HashtagAdvanced
    {
        public bool UseRandomLimit;
        public int From;
        public int To;
    }

    class WorkersSettings
    {
        public bool UseAdvancedSettings;
        public WorkerSettings[] Workers;
    }

    class WorkerSettings
    {
        public string Name;
        public bool UseProxyFromFile;
        public bool UseLocalIp;
        public bool UseCustomProxy;
        public Proxy Proxy;
        public bool UseSshReconnect;
        public SshClientSettings SshClient;
        public bool UseTcpClientReconnect;
        public TcpClientSettings TcpClient;
        public WorkerDirectSettings Direct;
        public FollowSenderWorkedSettings FollowSender;
    }

    class TcpClientSettings
    {
        public string Host;
        public int Port;
    }

    class SshClientSettings
    {
        public string Host;
        public string Port;
        public string Username;
        public string Password;
    }

    class WorkerDirectSettings
    {
        public bool Enable;
        public string SourceListName;
        public string SourceList;
        public bool TurnOnLikesSourceOnly;
        public bool UseSourceItemsAcceptedOnly;
        public string LikesSourceItemsAccepted;
        public bool UseCustomDomain;
        public string Domain;
        public bool LeaveSourceMarker;
        public int AccountsLimit;
        public int PauseSeconds;
    }

    class FollowSenderWorkedSettings
    {
        public bool Enable;
        public string UserProfilesSourceLabel;
        public string UserProfilesSource;
    }
}