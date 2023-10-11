namespace AutoGram
{
    static class Variables
    {
        // General
        public const string AppName = "Instagram AI";

        public const int AccountsWaitTimeout = 60000;
        public const int ErrorLimitPosting = 5;

        public const string MessengerTemplate = "@";

        // Programm Settings
        public static bool IsSharedVersion = false;
        public static bool IsUserYaroslav = false;

        public static bool IsFastVersion = true;
        public static bool IsImageProccessor = false;

        // UI
        public const int LogMaxSize = 400;

        // Pop Client
        public const string PopClientHostName = "pop.mail.ru";
        public const int PopClientPort = 995;
        public const bool PopClientSsl = true;

        // Files & Directories
//#if DEBUG
        //public const string FileSettings = "bin/Debug/settings.json";
        //public const string FileAdvancedSettings = "bin/Debug/advancedSettings.json";
        //public const string FileAccounts = "bin/Debug/accounts.txt";
        //public const string FileProxies = "bin/Debug/proxies.txt";
        //public const string FolderPhotos = "bin/Debug/Photos";
//#else
        public const string FileSettings = "settings.json";
        public const string FileAdvancedSettings = "advancedSettings.json";
        public const string FileAccounts = "accounts.txt";
        public const string FileProxies = "proxies.txt";
        public const string FolderPhotos = "Photos";
//#endif
        public const string FileDevices = "devices.json";
        public const string FilePhones = "bin/Database/devices.txt";
        public const string FileAdditionalTags = "bin/additional_tags.txt";
        public const string FileWords = "words.txt";
        public const string FileSentences = "sentences.txt";
        public const string FolderUsers = "Users";
        public const string FolderPosts = "Posts";
        public const string FolderStoriesMedia = "Stories/Media/";
        public const string FolderComments = "Comments/Users/";
        public const string FolderPhotosLite = "Lite";
        public const string FolderPhotosHard = "Hard";
        public const string FolderProfilePhoto = "ProfilePhoto";
        public const string FolderRegisterModule = "Registration";
        public const string FolderFonts = "bin/Fonts";
        public const string FileNames = "names.txt";
        public const string FileFirstNames = "first_names.txt";
        public const string FileCreatedAccounts = "accounts.txt";
        public const string FileCommandResetConnection = "bin/Commands/resetConnection.bat";
        public const string FileInstagramApps = "instagramApps.json";
        public const string ImageLogoKik = "bin/Images/kik.png";
        public const string FileLog = "bin/log.txt";
        public const string FileCommentsLog = "Comments/commentsLog.txt";
        public const string FileCommentsLinkLog = "Comments/commentsLinks.txt";
        public const string FileVerificationServiceLog = "bin/verification_service_log.txt";

        // Direct data
        public const string DirectFolder = "Direct";
        public const string DirectDatabases = "Databases";
        public const string DirectPhotos = "Photos";

        // Photo/Video Editor
        public const string FileVideoConvertor = "bin/ffmpeg.exe";

        // Notifications
        public const string NotificationIconPath = "bin/icon.png";
        public const int NotificationDesktopFrequency = -5;
        public const int NotificationTelegramFrequency = -30;
    }
}
