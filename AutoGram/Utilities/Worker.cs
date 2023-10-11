using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using AutoGram.Instagram.Exception;
using AutoGram.Storage.Model;
using AutoGram.Task;
using Org.BouncyCastle.Math.EC;

namespace AutoGram
{
    class Worker
    {
        public static List<Worker> All = new List<Worker>();

        public int Id;
        public UIAccount Account;
        public Thread Thread;
        public PhotoFolder Folder;
        public WorkerSettings Settings;
        public WorkerDirectSettings Direct;
        public FollowSenderWorkedSettings FollowSender;
        public bool IsSettings;
        public bool IsLite = true;

        private bool _isWork;
        public static event EventHandler UpdateStats;
        public static event EventHandler UpdateFollowersSearchStateEvent;
        public static event EventHandler UpdateAccountsStateEvent;

        // Registration counters
        public static int RegisteredSuccess;
        public static int RegisteredFailed;
        public int LocalRegistrationFailed;
        public int LocalRegistrationErrorsChain;

        // Direct counters
        public static int DirectSuccess;
        public static int DirectFailed;

        // Follow&Sender counters
        public static int FollowSenderFollowed;
        public static int FollowSenderFollowedEmpty;
        public static int FollowSenderFollowedFailed;

        // Followers search results
        public static int DirectFollowersSearchCorrect;
        public static int DirectFollowersSearchWrong;

        // Accounts state global
        public static int AccountsAuthSuccessfully;
        public static int AccountsBlocked;

        // Direct settings
        public readonly List<DirectSenderProfileSource> DirectSourceList;
        private int _sourceCounter;

        // FollowSender Settings
        public readonly List<UserProfileSource> UserProfileSourceList;
        private int _userProfilesSourceCounter;

        public Worker(UIAccount account, int id, WorkerSettings workerSettings = null)
        {
            Account = account;
            Id = id;

            Account.ControllerButton.Click += StopControl;

            IsWork = false;

            if (workerSettings != null)
            {
                Settings = workerSettings;
                Direct = workerSettings.Direct;
                FollowSender = workerSettings.FollowSender;
                IsSettings = true;

                if (Direct.Enable)
                {
                    var usernameSource = Direct.SourceList;
                    DirectSourceList = usernameSource.Split(' ').Distinct().Select(x => new DirectSenderProfileSource { UserPk = x, IsLimitedView = false }).ToList();
                    DirectSourceList.Shuffle();

                    account.UpdateDirectSource(workerSettings.Direct.SourceListName);
                }

                if (FollowSender.Enable)
                {
                    var userProfilesSource = FollowSender.UserProfilesSource;
                    UserProfileSourceList = userProfilesSource.Split(' ').Distinct().Select(x => 
                        new UserProfileSource { UserPk = x, IsLimitedView = false }
                    ).ToList();
                    UserProfileSourceList.Shuffle();

                    account.UpdateDirectSource(workerSettings.FollowSender.UserProfilesSourceLabel);
                }
            }
        }

        public void Destroy()
        {
            UnRegisterEvent();
            Account = null;
            Thread = null;

            if (Folder != null) Photos.UnreserveFolder(Folder);
            Folder = null;
        }

        public bool IsWork
        {
            get { return _isWork; }
            set
            {
                Account.ControllerButtonState(value);
                _isWork = value;
            }
        }

        public static void UpdateDirectStats()
        {
            UpdateStats?.Invoke($"Success {DirectSuccess} / Failed {DirectFailed}", EventArgs.Empty);
        }

        public static void UpdateFollowSenderStats()
        {
            UpdateStats?.Invoke($"Followed {FollowSenderFollowed} / {FollowSenderFollowedEmpty} / {FollowSenderFollowedFailed}", EventArgs.Empty);
        }

        public static void UpdateAccountsState()
        {
            UpdateAccountsStateEvent?.Invoke($"{AccountsAuthSuccessfully}:{AccountsBlocked}", EventArgs.Empty);
        }

        public static void UpdateFollowersSearchState()
        {
            UpdateFollowersSearchStateEvent?.Invoke($"{DirectFollowersSearchCorrect}:{DirectFollowersSearchWrong}", EventArgs.Empty);
        }

        public static void UpdateRegistrationStats()
        {
            UpdateStats?.Invoke($"Success {RegisteredSuccess} / Failed {RegisteredFailed}", EventArgs.Empty);
        }

        public void SkipActivate()
        {
            Account.Skip = false;
            Account.EnableSkipButton(true);
        }

        public void BindFolderPhotos()
        {
            if (Folder != null) Photos.UnreserveFolder(Folder);

            Folder = Photos.ReserveFolder(IsLite);
        }

        private void UnRegisterEvent()
        {
            Account.ControllerButton.Click -= StopControl;
        }

        private void SkipControl(object sender, EventArgs e)
        {
            Account.Skip = true;
            Account.EnableSkipButton(false);
        }

        private void StopControl(object sender, EventArgs e)
        {
            Button stopButton = sender as Button;
            if (stopButton == null) return;

            if (IsWork)
            {
                Thread.Abort();
                IsWork = false;
            }
            else
            {
                BindFolderPhotos();

                IsWork = true;
                Thread = new Thread(Work.Do);
                Thread.Start(this);
            }
        }

        private void LiteModeChecked(object sender, EventArgs e)
        {
            IsLite = true;

            if (!IsWork)
                BindFolderPhotos();
        }

        private void LiteModeUnchecked(object sender, EventArgs e)
        {
            IsLite = false;

            if (!IsWork)
                BindFolderPhotos();
        }

        public DirectSenderProfileSource GetProfileSource()
        {
            if (DirectSourceList.Count < 1)
            {
                throw new EmptySourceException();
            }

            if (DirectSourceList.Count > _sourceCounter)
                return DirectSourceList[_sourceCounter++];

            _sourceCounter = 0;
            return DirectSourceList[_sourceCounter++];
        }

        public UserProfileSource GetUserProfileSource()
        {
            if (UserProfileSourceList.Count < 1)
            {
                throw new EmptySourceException();
            }

            if (UserProfileSourceList.Count > _userProfilesSourceCounter)
                return UserProfileSourceList[_userProfilesSourceCounter++];

            _userProfilesSourceCounter = 0;
            return UserProfileSourceList[_userProfilesSourceCounter++];
        }
    }
}
