using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;
using AutoGram.Task.SubTask;

namespace AutoGram.Task
{
    class Main
    {
        // Lockers
        private static readonly object LockerAccountGetting = new object();
        private static readonly object LockerAccountsCounter = new object();
        private static readonly object LockerAddingUsedUsers = new object();
        private static readonly object LockerAccountSuspendCounter = new object();
        private static readonly object LockerAccountsStateUpdate = new object();

        private static readonly Queue<UserAccount> UserAccounts = new Queue<UserAccount>();
        private static readonly List<UserAccount> UsedUserAccounts = new List<UserAccount>();
        private static readonly List<UserAccount> LoadedUserAccounts = new List<UserAccount>();
        private static readonly HashSet<UserAccount> InProcessAccount = new HashSet<UserAccount>();
        private static readonly HashSet<UserAccount> BlockedAccounts = new HashSet<UserAccount>();

        private static int _currentAccountNumber;
        private static int _accountsLoopCounter;
        private static int _accountsSuspendCounter;

        static Main()
        {
            ImportUserAccounts();
        }

        private static void WaitAccounts()
        {
            while (!UserAccounts.Any())
            {
                ImportUserAccounts();

                if (UserAccounts.Any())
                    break;

                Notification.Show(NotificationException.AccountsEnded);
                Thread.Sleep(Variables.AccountsWaitTimeout);
            }
        }

        private static void ImportUserAccounts()
        {
            // Load user accounts data from file
            var usersData = File.ReadAllLines(Variables.FileAccounts);

            // To UserAccount.List
            var usersAccounts = (from userData in usersData
                                 where userData != String.Empty
                                 select new UserAccount(userData)).ToList();

            // Leave only unused user accounts
            var usersAccountsFiltered = usersAccounts.Where(a => !LoadedUserAccounts.Contains(a) && !BlockedAccounts.Contains(a))
                .ToList();

            // Loop all user accounts
            if (!usersAccountsFiltered.Any())
            {
                _accountsLoopCounter++;

                if (_accountsLoopCounter >= Settings.Advanced.General.AccountsLoops)
                    return;

                LoadedUserAccounts.Clear();

                while (InProcessAccount.Any())
                {
                    Thread.Sleep(5000);
                }

                ImportUserAccounts();
            }

            // Take X accounts for session
            if (usersAccountsFiltered.Count > Settings.Advanced.General.AccountsPerSession)
                usersAccountsFiltered = usersAccountsFiltered
                    .Take(Settings.Advanced.General.AccountsPerSession)
                    .ToList();

            // Enqueue accounts to queue
            foreach (var userAccount in usersAccountsFiltered)
            {
                UserAccounts.Enqueue(userAccount);
            }

            // Add their to blacklist
            LoadedUserAccounts.AddRange(usersAccountsFiltered);
        }

        public static void Do(Worker worker)
        {
            int maxUndefinedErrors = 5;
            int undefinedErrorsCounter = 0;

            bool resetConnection = true;
            int resetConnectionCounter = 0;

            int accountsExecutedSuccessfully = 0;

            // Accountscycle
            while (true)
            {
                try
                {
                    worker.Account.ClearInfo();

                    Instagram.Instagram user;
                    UserAccount userAccount;

                    #region Reconnect Settings

                    if (worker.IsSettings)
                    {
                        if (worker.Settings.UseSshReconnect)
                        {
                            worker.Account.WriteLog($"Reset ssh connection...");

                            try
                            {
                                ConsoleCommands.ResetSshConnection(worker.Settings.SshClient);
                            }
                            catch (Exception e)
                            {
                                worker.Account.WriteLog(e.Message);
                            }
                        }

                        if (worker.Settings.UseTcpClientReconnect/* && resetConnection*/)
                        {
                            worker.Account.WriteLog(new string('-', 63));
                            worker.Account.WriteLog("Reset network through tcp client...");

                            try
                            {
                                ConsoleCommands.ResetNetworkThroughTcpClient(worker.Settings.TcpClient);
                                worker.Account.WriteLog("Reconnecting through tcp client was successful.");

                            }
                            catch (Exception e)
                            {
                                worker.Account.WriteLog(e.Message);
                            }
                        }

                        resetConnection = true;
                    }

                    #endregion

                    #region Create account

                    if (Settings.Basic.Register.RegisterAccounts)
                    {
                        try
                        {
                            user = Register.Do(worker);

                            userAccount = new UserAccount
                            {
                                Username = user.Username,
                                Password = user.Password,
                                Email = user.Email
                            };

                            var sleepTime = Utils.Random.Next(Settings.Basic.Register.PauseFrom,
                                Settings.Basic.Register.PauseTo);

                            user.Log($"Sleep {sleepTime}s.");
                            Thread.Sleep(sleepTime * 1000);

                            if (!Settings.Advanced.Register.ContinueAfterRegistration)
                            {
                                undefinedErrorsCounter = 0;
                                continue;
                            }
                        }
                        catch (AccountCreateFailedException)
                        {
                            continue;
                        }
                        catch (AccountCreateLimitException)
                        {
                            worker.Account.WriteLog("Limit of registrations.");
                            Telegram.SendMessage($"Thread {worker.Id} has completed registration.");
                            break;
                        }
                        catch (AccountCreateFailedLimitException)
                        {
                            worker.Account.WriteLog("Limit of failed registrations.");
                            Telegram.SendMessage($"Thread {worker.Id} has completed registration.");
                            break;
                        }
                    }
                    #endregion
                    #region Initial Instagram Account

                    else
                    {
                        // Get account from file
                        lock (LockerAccountGetting)
                        {
                            if (UserAccounts.Any())
                            {
                                /*
                                if (Settings.Basic.Post.AccountsLooping)
                                {
                                    ImportUserAccounts();
                                }
                                */

                                // Get UserAccount from queue
                                userAccount = UserAccounts.Dequeue();
                                InProcessAccount.Add(userAccount);

                                try
                                {
                                    user = new Instagram.Instagram(userAccount.Username, userAccount.Password,
                                        userAccount.Email, worker: worker, userAccount: userAccount);

                                    if (!string.IsNullOrEmpty(userAccount.UrlProfile))
                                    {
                                        user.ProfileUrl = userAccount.UrlProfile;
                                    }
                                }
                                catch (DeviceFormatInvalidException)
                                {
                                    continue;
                                }

                                if (worker.IsSettings)
                                {
                                    if (worker.Settings.UseLocalIp)
                                    {
                                        if (user.IsProxy())
                                            user.RemoveProxy();
                                    }

                                    if (worker.Settings.UseCustomProxy)
                                    {
                                        user.SetProxy(worker.Settings.Proxy);
                                    }

                                    if (worker.Settings.UseProxyFromFile)
                                    {
                                        if (!user.IsProxy())
                                            Proxy.Set(user);
                                    }
                                }
                                else
                                {
                                    if (!user.IsProxy())
                                        Proxy.Set(user);
                                }

                                UIManager.AccountsEnded(false);
                                worker.Account.SetLink(user.Username);
                            }
                            else
                            {
                                worker.Account.WriteLog("Accounts ended. I am waiting for accounts.");

                                UIManager.AccountsEnded(true);
                                WaitAccounts();
                                continue;
                            }
                        }
                    }

                    #endregion

                    // Add account to the UsedUsers.List
                    lock (LockerAddingUsedUsers)
                    {
                        UsedUserAccounts.Add(userAccount);
                    }

                    var userResponse = new UserResponse();

                    // Instagram
                    try
                    {
                        // Login
                        if (!Settings.Basic.Register.RegisterAccounts)
                        {
                            Login.Do(user);
                        }

                        // Open Profile Screen
                        if (user.UserInfo == null)
                            user.UserInfo = user.Do(() => user.LiveAction.ShowProfileScreen());

                        #region Edit Profile

                        if (Settings.Advanced.PostAfterRegistration.Use
                            && Settings.Advanced.PostAfterRegistration.EditProfile
                            && !Settings.Advanced.PostAfterRegistration.EditProfileRandom

                            || Settings.Advanced.PostAfterRegistration.Use
                            && Settings.Advanced.PostAfterRegistration.EditProfile
                            && Settings.Advanced.PostAfterRegistration.EditProfileRandom
                            && Utils.UseIt()

                            || !Settings.Advanced.PostAfterRegistration.Use
                            && Settings.Basic.Instagram.EditProfile

                            || Settings.Basic.Instagram.EditProfile)
                        {
                            if (!user.Storage.ProfileEdited || Settings.Basic.Instagram.EditProfileEvenIfAlreadyEdited)
                            {
                                if (!user.Username.Contains("emily"))
                                {
                                    EditProfile.Do(user);
                                    user.FirstName = user.UserInfo.User.Full_name.Split(' ').Length > 0
                                        ? user.UserInfo.User.Full_name.Split(' ')[0]
                                        : user.UserInfo.User.Full_name;
                                    worker.Account.SetName(user.Username);
                                    Utils.RandomSleep(4000, 6000);
                                }
                            }
                        }

                        #endregion

                        #region Change Profile Picture

                        if (Settings.Basic.Instagram.UploadProfilePhoto)
                        {
                            if (!user.Storage.ProfilePhotoChanged
                                || Settings.Basic.Instagram.UploadProfilePhotoEvenIfAlreadyUploaded)
                            {
                                bool loadEditProfileScreen = !Settings.Advanced.General.IsEconomyMode;

                                var profilePhoto = Photos.GetProfilePhoto(worker.Folder);
                                ChangeProfilePicture.Do(user, new Photo(profilePhoto, false, true), loadEditProfileScreen: loadEditProfileScreen);

                                // Update profile screen
                                if (Settings.Advanced.General.SyncUiWithInstagramAccount)
                                {
                                    userResponse = user.Do(() => user.LiveAction.ShowProfileScreen());
                                    worker.Account.UpdateProfileImage(userResponse.User.Profile_pic_url);
                                }
                            }
                        }

                        #endregion

                        if (Settings.Advanced.CreatorChildAccounts.Enable)
                        {
                            try
                            {
                                CreateChildAccounts.Do(user);
                            }
                            catch (AccountCreateFailedException)
                            {
                                continue;
                            }
                            catch (AccountCreateFailedLimitException)
                            {
                                worker.Account.WriteLog("Limit of failed registrations.");
                                Telegram.SendMessage($"Thread {worker.Id} has completed registration.");
                                break;
                            }
                        }

                        if (Settings.Advanced.Profile.Stories.UploadStories)
                        {
                            Stories.UploadStories(user);
                        }

                        // Direct
                        if (Settings.Advanced.Direct.Enable)
                        {
                            Direct.Do(user);
                        }

                        // Live settings
                        if (Settings.Advanced.Live.Enable)
                        {
                            Live.Do(user);

                            Utils.RandomSleep(3000, 10000);
                        }

                        // Bulk posting
                        if (Settings.Advanced.Post.Enable)
                        {
                            if (Settings.Advanced.Post.DontPublishIfPostsDeletedAutomatically
                                && !user.Storage.IsPostsDeletedAutomatically ||
                                !Settings.Advanced.Post.DontPublishIfPostsDeletedAutomatically)
                            {
                                BulkPosting.Do(worker, user);

                                if (Settings.Advanced.Post.CheckDirectAfterPosting)
                                {
                                    var waitingTime = Settings.Advanced.Post.WaitDirect * 60 * 1000;

                                    user.Log($"Sleep {Settings.Advanced.Post.WaitDirect} m.");
                                    Thread.Sleep(waitingTime);

                                    Direct.Do(user);
                                }
                            }
                        }

                        // Bulk commenting
                        if (Settings.Advanced.Comment.Enable
                            && user.Activity.Session.CommentingTaskExecutionCounter <
                            Settings.Advanced.Comment.TaskCountPerSession)
                        {
                            try
                            {
                                BulkCommenting.Do(worker, user);
                            }
                            catch (SuspendTaskException)
                            {
                                user.Activity.Session.CommentingTaskExecutionCounter++;
                                user.Log($"Commenting finished.");
                            }

                            if (Settings.Advanced.Comment.CheckDirectAfterCommenting)
                            {
                                var waitingTime = Settings.Advanced.Comment.WaitDirect * 60 * 1000;

                                user.Log($"Sleep {Settings.Advanced.Comment.WaitDirect} m.");
                                Thread.Sleep(waitingTime);

                                Direct.Do(user);
                            }
                        }

                        // StoryViewer
                        if (Settings.Advanced.StoryViewer.Enable)
                        {
                            try
                            {
                                StoryViewer.Do(user);
                            }
                            catch (SuspendTaskException)
                            {
                                user.Log($"Stories viewing finished.");
                            }
                        }

                        // DirectSender
                        if (Settings.Advanced.DirectSender.Enable)
                        {
                            try
                            {
                                var cycleCounter = 0;
                                while (true)
                                {
                                    DirectSender.Do(user, Settings.Advanced.DirectSender.ParticipantsCount);

                                    cycleCounter++;

                                    if (cycleCounter >= Settings.Advanced.DirectSender.Count)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (SuspendTaskException e)
                            {
                                user.Log($"Direct sender work is finished.");
                            }
                        }

                        // Follow Sender
                        if (Settings.Advanced.FollowSender.Enable)
                        {
                            try
                            {
                                FollowSender.Do(user);
                            }
                            catch (SuspendTaskException e)
                            {
                                user.Log($"Follow&Sender work is finished.");
                            }
                        }

                        // FollowersParser
                        if (Settings.Advanced.FollowersParser.Enable)
                        {
                            try
                            {
                                FollowersParser.Do(user);
                            }
                            catch (SuspendTaskException e)
                            {
                                user.Log($"Followers parser work is finished.");
                            }
                        }

                        // ProfileChecker
                        if (Settings.Advanced.ProfileChecker.Enable)
                        {
                            try
                            {
                                ProfileChecker.Do(user);
                            }
                            catch (SuspendTaskException e)
                            {
                                user.Log($"Profile checking work is finished.");
                            }
                        }
                    }
                    catch (InvalidCredentialsException)
                    {
                        user.Log("Wrong username/password.");
                        user.IsSuspended = true;
                    }
                    catch (PhoneVerificationChallengeException)
                    {
                        user.Log("Phone verification challenge.");
                        user.IsPhoneChallenge = true;
                        user.IsSuspended = true;
                    }
                    catch (EmailVerificationChallengeException)
                    {
                        user.Log("Email verification challenge.");
                        user.IsEmailChallenge = true;
                        user.IsSuspended = true;
                    }
                    catch (DeletedContentChallengeException)
                    {
                        user.Log("Deleted content challenge.");
                        user.IsDeletedContentChallenge = true;
                        user.IsSuspended = true;
                    }
                    catch (ChallengeRequiredException)
                    {
                        user.Log("Challenge required.");
                        user.IsUndefinedChallenge = true;
                        user.IsSuspended = true;

                        resetConnection = false;
                        resetConnectionCounter++;

                        if (resetConnectionCounter >= 3)
                        {
                            resetConnection = true;
                            resetConnectionCounter = 0;
                        }
                    }
                    catch (LoginRequiredException)
                    {
                        user.Log("Login required exception.");
                        user.IsSuspended = true;
                    }
                    catch (InactiveUserException)
                    {
                        user.Log("Your account has been suspended for violating our terms. " +
                                 "Learn how to recover your account.");
                        user.IsSuspended = true;
                    }
                    catch (SuspendExecutionException)
                    {
                        // Do something...
                    }
                    catch (SomethingWrongException exception)
                    {
                        user.Log(exception.Message);
                    }
                    finally
                    {
                        InProcessAccount.Remove(userAccount);

                        if (user.Storage != null && !user.DisableStorageSaving)
                        {
                            user.Storage.Save();
                        }

                        if (user.IsSuspended)
                        {
                            BlockedAccounts.Add(userAccount);
                        }

                        lock (LockerAccountSuspendCounter)
                        {
                            if (user.IsSuspended)
                            {
                                _accountsSuspendCounter++;

                                Worker.AccountsBlocked++;
                            }
                            else
                            {
                                _accountsSuspendCounter = 0;

                                Worker.AccountsAuthSuccessfully++;
                                accountsExecutedSuccessfully++;
                            }

                            Worker.UpdateAccountsState();
                            user.Worker.Account.UpdateAccountCount(accountsExecutedSuccessfully);

                            if (user.Worker.IsSettings && user.Worker.Direct.AccountsLimit != 0)
                            {
                                if (accountsExecutedSuccessfully >= user.Worker.Direct.AccountsLimit)
                                    throw new SuspendThreadWorkException("Direct finished.");
                            }

                            if (_accountsSuspendCounter > 40)
                                throw new SuspendThreadWorkException("More than 10 accounts are suspended successively.");
                        }

                        if (Settings.Advanced.General.UsePauseBetweenAccounts)
                        {
                            var accountSleepInterval = Convert.ToInt32(Settings.Advanced.General.PauseFrom);
                            user.Log($"Sleep {accountSleepInterval} s");
                            Thread.Sleep(accountSleepInterval * 1000);
                        }
                    }

                    undefinedErrorsCounter = 0;
                }
                catch (SuspendThreadWorkException exception)
                {
                    Telegram.SendMessage($"Suspend work thread. Error message: {exception.Message}");

                    return;
                }
                catch (Exception exception)
                {
                    if (exception.Message.Contains("Поток находился")
                        || exception.Message.Contains("Thread was being"))
                    {
                        if (worker.Thread != null && worker.Thread.IsAlive)
                            worker.Thread.Abort();

                        return;
                    }

                    if (undefinedErrorsCounter >= maxUndefinedErrors)
                    {
                        worker.Account.WriteLog("All actions are suspended. Undefined errors limit.");
                        Telegram.SendMessage("All actions are suspended. Undefined errors limit.");
                        return;
                    }

                    worker.Account.WriteLog(exception.Message);
                    Log.Write($"{exception.Message} | Stacktrace: {exception.StackTrace}", LogResource.General);

                    undefinedErrorsCounter++;
                }
            }
        }
    }
}
