using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Database.DirectSender;
using AutoGram.Helpers;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response.Friendship;
using AutoGram.Instagram.Response.Model;
using AutoGram.Services;
using AutoGram.Task.SubTask;
using Newtonsoft.Json;

namespace AutoGram.Task
{
    public class InstagramDatabaseProfileData
    {
        public string Pk { get; set; }
        public string Username { get; set; }

        public string Url { get; set; }
    }

    static class FollowersParser
    {
        private static readonly Queue<string> UsernameList;
        private static readonly object DataLocker = new object();
        private static readonly object DataBaseLocker = new object();
        private static readonly List<InstagramDatabaseProfileData> FollowingsList = new List<InstagramDatabaseProfileData>();
        private static readonly List<string> FollowingsUrlList = new List<string>();


        private static readonly HashSet<UserDirect> AllUserList = new HashSet<UserDirect>();
        private static readonly List<string> AcceptedUserList = new List<string>();
        private static readonly List<string> RejectedUserList = new List<string>();
        private static readonly List<string> RejectedByWhiteList = new List<string>();
        private static readonly List<string> AcceptedByWhiteList = new List<string>();

        static FollowersParser()
        {
            var usernameSource = Settings.Advanced.FollowersParser.UsernameSource;
            UsernameList = new Queue<string>(usernameSource.Split(' ').ToList());
        }

        public static void Do(Instagram.Instagram user)
        {
            while (true)
            {
                string userPk = string.Empty;

                lock (DataLocker)
                {
                    if (UsernameList.Any())
                        userPk = UsernameList.Dequeue();
                    else
                    {
                        user.Log("Usernames for parsing are ended. Finished.");

                        user.Log("Saving data...");

                        File.WriteAllLines($"FilterResults/acceptedUsers.txt", AcceptedUserList);
                        File.WriteAllLines($"FilterResults/rejectedUsers.txt", RejectedUserList);
                        File.WriteAllLines($"FilterResults/rejectedByWL.txt", RejectedByWhiteList);
                        File.WriteAllLines($"FilterResults/acceptedByWL.txt", AcceptedByWhiteList);

                        throw new SuspendThreadWorkException();
                    }
                }

                var targetUser = new User { Pk = userPk };
                var profileResponse = Profile.Open(user, targetUser);

                var targetUsername = profileResponse.UserInfo.User.Username;

                user.Log($"Opening profile {targetUsername}");

                string nextMaxId = string.Empty;
                string rankToken = Utils.GenerateUUID(true);
                int seenChainCounter = 0;

                int founded = 0;

                while (true)
                {
                    if (Settings.Advanced.FollowersParser.ParseFollowings)
                    {
                        user.Log($"Loading following of {targetUsername}");

                        // Load following
                        var friendshipsResponse = user.Do(() =>
                            user.FriendShips.GetFollowings(profileResponse.UserInfo.User.Pk, rankToken, nextMaxId));

                        // Load friendships statuses
                        string userIds = friendshipsResponse.Users.Select(u => u.Pk).Aggregate((x, y) => $"{x},{y}");
                        user.Do(() => user.FriendShips.ShowMany(userIds));

                        bool next = false;
                        int added = 0;

                        foreach (var follower in friendshipsResponse.Users)
                        {
                            lock (DataLocker)
                            {
                                if (FollowingsList.All(x => x.Pk != follower.Pk))
                                {
                                    FollowingsList.Add(new InstagramDatabaseProfileData { Pk = follower.Pk, Username = follower.Username, Url = $"https://instagram.com/{follower.Username}" });
                                    FollowingsUrlList.Add($"https://instagram.com/{follower.Username}/");
                                    founded++;
                                }
                            }

                            if (founded >= Settings.Advanced.FollowersParser.MaximumFromEach)
                            {
                                next = true;
                                break;
                            }
                        }

                        user.Log($"Added to database followers: {founded}");

                        if (friendshipsResponse.IsNextMaxId)
                        {
                            nextMaxId = friendshipsResponse.NextMaxId;
                        }
                        else break;

                        if (next) break;
                    }
                    else
                    {
                        user.Log($"Loading followers of {targetUsername}");

                        bool next = false;
                        int added = 0;

                        FriendshipsResponse friendshipsResponse;

                        try
                        {
                            // Load followers
                            friendshipsResponse = user.Do(() =>
                                user.FriendShips.GetFollowers(profileResponse.UserInfo.User.Pk, rankToken, nextMaxId));

                            // Load friendships statuses
                            string userIds = friendshipsResponse.Users.Select(u => u.Pk).Aggregate((x, y) => $"{x},{y}");
                            user.Do(() => user.FriendShips.ShowMany(userIds));
                        }
                        catch
                        {
                            user.Log(targetUser.Pk);
                            continue;
                        }

                        lock (DataBaseLocker)
                        {
                            HashSet<UserDirect> allUsers = new HashSet<UserDirect>();

                            foreach (var userDirect in friendshipsResponse.Users.ConvertToUserDirects())
                            {
                                if (AllUserList.All(x => x.Username != userDirect.Username))
                                {
                                    allUsers.Add(userDirect);
                                    AllUserList.Add(userDirect);
                                }
                            }

                            // Filtration

                            // accepted
                            var usersAcepted = allUsers
                                .Where(u => u.IsAccepted(Settings.Advanced.DirectSender.Filtration))
                                .ToList();

                            AcceptedUserList.AddRange(usersAcepted.Select(x => $"{x.FullName} | {x.Username}"));
                            user.Log($"Accepted users: {usersAcepted.Count}");


                            // rejected
                            var usersRejected = allUsers
                                .Where(u => !u.IsAccepted(Settings.Advanced.DirectSender.Filtration))
                                .ToList();

                            RejectedUserList.AddRange(usersRejected.Select(x => $"{x.FullName} | {x.Username}"));
                            user.Log($"Rejected users: {usersRejected.Count}");

                            // rejected by whitelist
                            var rejectedByWhiteList = usersAcepted
                                .Where(u => !u.IsContainsInWhiteList())
                                .ToList();

                            RejectedByWhiteList.AddRange(
                                rejectedByWhiteList.Select(x => $"{x.FullName} | {x.Username}"));
                            user.Log($"Rejected by white list users: {rejectedByWhiteList.Count}");

                            // accepted by whitelist
                            var acceptedByWhiteList = usersAcepted
                                .Where(u => u.IsContainsInWhiteList(Settings.Advanced.DirectSender.Filtration.UseSurnameWhiteList))
                                .ToList();

                            AcceptedByWhiteList.AddRange(
                                acceptedByWhiteList.Select(x => $"{x.FullName} | {x.Username}"));
                            user.Log($"Accepted by white list users: {acceptedByWhiteList.Count}");
                        }

                        founded++;

                        if (founded >= Settings.Advanced.FollowersParser.MaximumFromEach)
                        {
                            next = true;
                            break;
                        }

                        if (friendshipsResponse.IsNextMaxId)
                        {
                            nextMaxId = friendshipsResponse.NextMaxId;
                        }
                        else break;

                        if (next) break;
                    }

                    user.Log($"Sleep {Settings.Advanced.FollowersParser.PauseMilliseconds} ms");
                    Thread.Sleep(Settings.Advanced.FollowersParser.PauseMilliseconds);
                }
            }
        }
    }
}
