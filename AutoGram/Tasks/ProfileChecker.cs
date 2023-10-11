using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response.Model;
using AutoGram.Task.SubTask;

namespace AutoGram.Task
{
    static class ProfileChecker
    {
        private static readonly Queue<string> ProfileList;
        private static List<ProfileResult> _profileResults = new List<ProfileResult>();
        private static readonly object DataLocker = new object();

        private static readonly string SaveFilename = $"Results_at_{Utils.DateTimeNowTotalSeconds}.txt";

        static ProfileChecker()
        {
            var profiles = Settings.Advanced.ProfileChecker.ProfileList.Split(' ').Distinct().ToList();
            ProfileList = new Queue<string>(profiles);
        }

        public static void Do(Instagram.Instagram user)
        {
            while (true)
            {
                string userPk = string.Empty;

                lock (DataLocker)
                {
                    if (ProfileList.Any())
                        userPk = ProfileList.Dequeue();
                    else
                    {
                        user.Log("Profiles for checking are ended. Finished.");
                        user.Log("Saving data...");

                        var outputData = string.Join(" ", _profileResults.OrderByDescending(p => p.Followers).Select(x => x.Pk));

                        outputData += "\n\n";
                        outputData += string.Join("\n", _profileResults.OrderByDescending(p => p.Followers));

                        File.WriteAllText($"ProfileChecker/{SaveFilename}", outputData);

                        throw new SuspendThreadWorkException();
                    }
                }

                var targetUser = new User { Pk = userPk };

                Profile.ProfileResponse profileResponse;

                try
                {
                    profileResponse = Profile.Open(user, targetUser);

                    var targetUsername = profileResponse.UserInfo.User.Username;

                    user.Log($"Opening profile {targetUsername}");

                    if (profileResponse.UserInfo.User.IsPrivate)
                    {
                        throw new LoadingFollowersException();
                    }
                }
                catch (LoadingFollowersException)
                {
                    user.Log($"User pk {targetUser.Pk} is private.");

                    continue;
                }
                catch (UserNotFoundException)
                {
                    user.Log($"User pk {targetUser.Pk} not found.");

                    continue;
                }

                var profileResult = new ProfileResult
                {
                    Pk = userPk,
                    Followers = profileResponse.UserInfo.User.Follower_count,
                    Url = $"https://www.instagram.com/{profileResponse.UserInfo.User.Username}"
                };

                user.Log($"Result: {profileResult}");

                _profileResults.Add(profileResult);

                Thread.Sleep(2000);
            }
        }
    }

    class ProfileResult
    {
        public string Url;
        public string Pk;
        public int Followers;

        public override string ToString()
        {
            return $"{Url} [{Pk}] ({Followers}+)";
        }
    }
}
