using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using Database;
using Database.DirectSender;
using Database.Model;
using AutoGram.Helpers;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Friendship;
using AutoGram.Instagram.Response.Model;
using AutoGram.Services;
using AutoGram.Task.SubTask;
using Newtonsoft.Json;
using xNet;

namespace AutoGram.Task
{
    static class FollowSender
    {
        private static readonly List<UserProfileSource> UserProfileSourceList;
        private static int _userProfileSourceCounter;

        // Lockers
        private static readonly object DatabaseLocker = new object();
        private static readonly object DataLocker = new object();
        private static readonly object SyncBlackListLocker = new object();
        private static readonly object BlackListLocker = new object();
        private static readonly object UpdateStatsLock = new object();

        // Black Lists 
        private static readonly HashSet<UserDirect> BlackList;
        public static readonly HashSet<UserDirect> TemporaryBlackList = new HashSet<UserDirect>();
        private static readonly HashSet<UserDirect> LocalRejectedList = new HashSet<UserDirect>();
        private static readonly HashSet<UserDirect> CancelledUserList = new HashSet<UserDirect>();

        static FollowSender()
        {
            var userProfilesSource = Settings.Advanced.FollowSender.UserProfilesSource;
            UserProfileSourceList = userProfilesSource.Split(' ').Distinct().Select(x =>
                new UserProfileSource { UserPk = x, IsLimitedView = false }
            ).ToList();
            UserProfileSourceList.Shuffle();

            BlackList = new HashSet<UserDirect>(UsersDirectBlacklistRepository.GetAll());
        }

        public static void Do(Instagram.Instagram user)
        {
            bool minimizeRequests = AutoGram.Settings.Advanced.General.MinimizeInstagramRequests;

            // Following users

            int followedAlreadyCounter = 0;
            int followPerActionLimit = Settings.Advanced.FollowSender.FollowSettings.FollowPerActionLimit;
            int followLimit = Settings.Advanced.FollowSender.FollowSettings.FollowFromEachAccountLimit;

            if (user.Storage.IsFollowSenderFinished)
            {
                throw new SuspendTaskException();
            }

            if (user.Storage.FollowSenderFollowingBy >= followLimit)
            {
                user.Storage.IsFollowSenderFinished = true;
                user.Storage.Save();

                throw new SuspendTaskException();
            }

            // Real Api Requests
            if (!minimizeRequests)
            {
                user.State.IgNavChain = "24F:explore_popular:2";
                user.Do(() => user.Internal.FbRecentSearches());
                user.Do(() => user.Internal.CommerceDestination());
                user.Do(() => user.Internal.FbSearchNullStateDynamicSections());
                user.Do(() => user.Discover.TopicalExplore());
                Utils.RandomSleep(4000, 6000);
            }

            while (true)
            {
                // Parsing

                var usersToFollowQueue = ParseUsersFromUserProfileSource(user,
                    Settings.Advanced.FollowSender.FollowSettings.SearchAndFollowLoopLimit);

                // Following

                bool finishAction = false;

                user.Log("Following target users...");
                while (usersToFollowQueue.Any())
                {
                    var userToFollow = usersToFollowQueue.Dequeue();

                    var followResponse = user.Do(() => user.FriendShips.Create(userToFollow.Pk));

                    if (followResponse.IsOk())
                    {
                        if (followResponse.FriendshipCreateStatus.Following)
                        {
                            user.Log($"Following user {userToFollow.Username}.");
                        }
                        else
                        {
                            user.Log($"[Private user] Following user {userToFollow.Username}.");
                        }

                        user.Storage.FollowSenderFollowingBy++;
                        followedAlreadyCounter++;

                        lock (UpdateStatsLock)
                        {
                            Worker.FollowSenderFollowed++;
                            Worker.UpdateFollowSenderStats();
                        }

                        if (user.Storage.FollowSenderFollowingBy >= followLimit)
                        {
                            user.Log($"[Success] Follow Sender for this account is finished.");
                            user.Storage.IsFollowSenderFinished = true;
                            finishAction = true;
                            break;
                        }

                        if (followedAlreadyCounter >= followPerActionLimit)
                        {
                            user.Log($"[Action Success] Follow Sender for this action is finished.");
                            finishAction = true;
                            break;
                        }
                    }
                    else
                    {
                        string errorMessage = followResponse.IsMessage()
                            ? followResponse.GetMessage()
                            : "Follow error.";

                        lock (UpdateStatsLock)
                        {
                            Worker.FollowSenderFollowedFailed++;
                            Worker.UpdateFollowSenderStats();
                        }

                        Log.Write(errorMessage, LogResource.Live);
                        user.Log(errorMessage);
                    }

                    user.Log($"Sleep {Settings.Advanced.FollowSender.FollowSettings.PauseBetweenFollowing}s");
                    Thread.Sleep(Settings.Advanced.FollowSender.FollowSettings.PauseBetweenFollowing * 1000);
                }

                if (finishAction)
                {
                    lock (SyncBlackListLocker)
                    {
                        while (usersToFollowQueue.Any())
                        {
                            var userToFollow = usersToFollowQueue.Dequeue();
                            CancelledUserList.Add(userToFollow);
                        }
                    }

                    user.Storage.Save();
                    throw new SuspendTaskException();
                }

                user.Log($"Sleep {Settings.Advanced.FollowSender.FollowSettings.PauseBetweenLoopInSeconds}s");
                Thread.Sleep(Settings.Advanced.FollowSender.FollowSettings.PauseBetweenLoopInSeconds * 1000);
            }
        }

        private static Queue<UserDirect> ParseUsersFromUserProfileSource(Instagram.Instagram user, int usersForParsingLimit)
        {
            var parsedUsersList = new Queue<UserDirect>();

            while (true)
            {
                UserProfileSource userProfileSource;

                try
                {
                    userProfileSource = user.Worker.IsSettings && user.Worker.FollowSender.Enable
                                    ? user.Worker.GetUserProfileSource()
                                    : GetUserProfileSource();
                }
                catch (EmptySourceException)
                {
                    user.Log("Follow&Sender source is empty.");
                    throw new SuspendThreadWorkException();
                }

                if (userProfileSource.Disabled)
                    continue;

                // Filter User Profiles Sources
                if (userProfileSource.Muted)
                {
                    userProfileSource.CurrentMuteCounter++;

                    if (userProfileSource.CurrentMuteCounter >= userProfileSource.MuteLimit)
                    {
                        userProfileSource.Muted = false;
                        userProfileSource.CurrentMuteCounter = 0;
                        userProfileSource.EndMuteTime = DateTime.Now;
                    }
                    else
                    {
                        continue;
                    }
                }

                var userProfile = new User { Pk = userProfileSource.UserPk };

                Profile.ProfileResponse profileResponse;

                try
                {
                    profileResponse = Profile.Open(user, userProfile);

                    if (profileResponse.UserInfo.User.IsPrivate)
                    {
                        throw new LoadingFollowersException();
                    }
                }
                catch (OpenProfileException)
                {
                    continue;
                }
                catch (ArgumentNullException ex)
                {
                    user.Log(ex.Message);
                    continue;
                }
                catch (LoadingFollowersException)
                {
                    user.Log($"User pk {userProfile.Pk} is private.");

                    lock (DataLocker)
                    {
                        if (user.Worker.IsSettings && user.Worker.FollowSender.Enable)
                        {
                            var tempUserProfileSource = user.Worker.UserProfileSourceList.FirstOrDefault(
                                x => x.UserPk == userProfileSource.UserPk);
                            user.Worker.UserProfileSourceList.Remove(tempUserProfileSource);
                        }
                        else
                        {
                            var tempUserProfileSource = UserProfileSourceList.FirstOrDefault(x => x.UserPk == userProfileSource.UserPk);
                            UserProfileSourceList.Remove(tempUserProfileSource);
                        }
                    }

                    continue;
                }
                catch (UserNotFoundException)
                {
                    user.Log($"User pk {userProfile.Pk} not found.");

                    lock (DataLocker)
                    {
                        if (user.Worker.IsSettings && user.Worker.FollowSender.Enable)
                        {
                            var tempUserProfileSource = user.Worker.UserProfileSourceList.FirstOrDefault(
                                x => x.UserPk == userProfileSource.UserPk);
                            user.Worker.UserProfileSourceList.Remove(tempUserProfileSource);
                        }
                        else
                        {
                            var tempUserProfileSource = UserProfileSourceList.FirstOrDefault(x => x.UserPk == userProfileSource.UserPk);
                            UserProfileSourceList.Remove(tempUserProfileSource);
                        }
                    }

                    continue;
                }

                var userProfileUsername = profileResponse.UserInfo.User.Username;

                // API: Open User Profile

                user.Log($"Opening profile {userProfileUsername}");

                string nextMaxId = string.Empty;
                string rankToken = Utils.GenerateUUID(true);

                // Is Enough Users for Parsing
                bool isEnoughUsers = false;

                int deepFollowersLoad = 0;
                int allowedParticipants = 0;
                int allNewUsersFromSource = 0;

                FriendshipsResponse friendshipsResponse;

                // Parse Users

                var usersFiltered = new List<UserDirect>();

                // Load Followers
                friendshipsResponse = user.Do(() =>
                    user.FriendShips.GetFollowers(profileResponse.UserInfo.User.Pk, rankToken, nextMaxId));

                // Filtration
                usersFiltered = friendshipsResponse
                    .Users
                    .ConvertToUserDirects()
                    .Where(u => u.IsAccepted(Settings.Advanced.FollowSender.Filtration))
                    .ToList();

                if (Settings.Advanced.DirectSender.Filtration.Enable)
                {
                    if (Settings.Advanced.DirectSender.Filtration.UseWhiteList)
                    {
                        usersFiltered = usersFiltered
                            .Where(u => u.IsContainsInWhiteList(Settings.Advanced.DirectSender.Filtration.UseSurnameWhiteList))
                            .ToList();
                    }
                }

                int founded = 0;
                int newUsersCounter = 0;

                lock (DatabaseLocker)
                {
                    var noRejectedUsers = usersFiltered.Where(u => LocalRejectedList.All(a => a.Pk != u.Pk)).ToList();

                    if (noRejectedUsers.Any())
                    {
                        var acceptedUsers = noRejectedUsers.Where(u => BlackList.All(b => b.Pk != u.Pk)).ToList();

                        // Local rejected users database
                        var rejectedUsers = noRejectedUsers.Where(u => acceptedUsers.All(a => a.Pk != u.Pk)).ToList();

                        foreach (var rejectedUser in rejectedUsers)
                        {
                            LocalRejectedList.Add(rejectedUser);
                        }

                        newUsersCounter = acceptedUsers.Count;
                        allNewUsersFromSource += acceptedUsers.Count;

                        var takedUsers = new List<UserDirect>();

                        foreach (var acceptedUser in acceptedUsers)
                        {
                            takedUsers.Add(acceptedUser);
                            parsedUsersList.Enqueue(acceptedUser);
                            allowedParticipants++;
                            founded++;

                            // Add to blacklist
                            BlackList.Add(acceptedUser);
                            TemporaryBlackList.Add(acceptedUser);

                            if (parsedUsersList.Count >= usersForParsingLimit)
                            {
                                isEnoughUsers = true;
                                break;
                            }
                        }

                        if (TemporaryBlackList.Count >= 200)
                        {
                            lock (SyncBlackListLocker)
                            {
                                foreach (var cancelledUser in CancelledUserList)
                                {
                                    if (BlackList.Any(x => x.Pk == cancelledUser.Pk))
                                    {
                                        BlackList.RemoveWhere(x => x.Pk == cancelledUser.Pk);
                                    }

                                    if (TemporaryBlackList.Any(x => x.Pk == cancelledUser.Pk))
                                    {
                                        TemporaryBlackList.RemoveWhere(x => x.Pk == cancelledUser.Pk);
                                    }
                                }

                                CancelledUserList.Clear();
                            }

                            if (TemporaryBlackList.Any())
                                UsersDirectBlacklistRepository.AddRange(TemporaryBlackList.ToList());

                            TemporaryBlackList.Clear();
                        }
                    }
                }

                user.Log($"{usersFiltered.Count} are accepted by filtration, new users {newUsersCounter}, taken {founded}. Already {parsedUsersList.Count}");

                if (Settings.Advanced.DirectSender.MuteLevelsIsEnabled)
                {
                    if (allNewUsersFromSource <= 2)
                    {
                        if (string.IsNullOrEmpty(userProfileSource.Username)) userProfileSource.Username = userProfileUsername;

                        userProfileSource.Muted = true;

                        if (userProfileSource.MuteLevel == 1)
                        {
                            userProfileSource.MuteLimit = 15;
                        }
                        else if (userProfileSource.MuteLevel == 2)
                        {
                            userProfileSource.MuteLimit = 25;
                        }
                        else if (userProfileSource.MuteLevel == 3)
                        {
                            userProfileSource.MuteLimit = 35;
                        }
                        else if (userProfileSource.MuteLevel == 4)
                        {
                            userProfileSource.MuteLimit = 45;
                        }
                        else if (userProfileSource.MuteLevel >= 5)
                        {
                            userProfileSource.MuteLimit = 60;
                        }
                        else userProfileSource.MuteLimit = 10;

                        userProfileSource.BeginMuteTime = DateTime.Now;
                        userProfileSource.MuteLevel++;
                    }
                    else
                    {
                        if (userProfileSource.MuteLevel > 0)
                        {
                            userProfileSource.MuteLevel = 0;
                        }
                    }
                }

                if (isEnoughUsers) break;
            }

            return parsedUsersList;
        }

        private static UserProfileSource GetUserProfileSource()
        {
            lock (DataLocker)
            {
                if (UserProfileSourceList.Count < 1)
                {
                    throw new EmptySourceException();
                }

                if (UserProfileSourceList.Count > _userProfileSourceCounter)
                    return UserProfileSourceList[_userProfileSourceCounter++];

                _userProfileSourceCounter = 0;
                return UserProfileSourceList[_userProfileSourceCounter++];
            }
        }
    }

    public class UserProfileSource
    {
        public string UserPk { get; set; }
        public string Username { get; set; }
        public bool Muted { get; set; }
        public int MuteLevel { get; set; }
        public int MuteLimit { get; set; }
        public int CurrentMuteCounter { get; set; }
        public DateTime BeginMuteTime { get; set; }
        public DateTime EndMuteTime { get; set; }
        public bool IsLimitedView { get; set; }
        public bool Disabled { get; set; }
    }
}
