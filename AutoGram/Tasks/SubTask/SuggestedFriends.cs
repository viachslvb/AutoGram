using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;

namespace AutoGram.Task.SubTask
{
    class SuggestedFriends
    {
        private static readonly List<string> AllowUsersList;
        private static readonly object LockSavingSuggestionUsers = new object();

        static SuggestedFriends()
        {
            string allowUsers = Settings.Advanced.Live.SuggestedFriends.AllowUsersOnStarting;
            AllowUsersList = allowUsers.Replace(" ", "").Split(',').ToList();
        }

        public static void Explore(Instagram.Instagram user)
        {
            user.Log("Looking for suggested friends.");

            // Open SuggestedFriends page
            var suggestedFriends = user.Do(() => GetSuggestions(user));

            // Check FB suggestions
            user.Do(() => user.Internal.CheckFbFriends());

            // Settings
            bool isMarkedSeen = false;
            bool followLimit = false;
            bool exploreProfilesLimit = false;

            if (!suggestedFriends.IsOk())
            {
                string errorMessage = suggestedFriends.IsMessage()
                    ? suggestedFriends.Message
                    : "GetSuggestions method exception.";

                Log.Write(errorMessage, LogResource.SuggestedFriend);
                throw new SomethingWrongException(errorMessage);
            }

            do
            {
                if (user.LiveSettings.Follow.IsLimit)
                {
                    user.Log($"Followings are limited.");
                    break;
                }

                if (!suggestedFriends.GetSuggestedUsers
                    .Any(s => !s.User.Following && !s.User.Status.OutComingRequest))
                    break;

                // Save suggestion for list creating
                SaveSuggestionUsers(suggestedFriends.GetSuggestedUsers.ToList());

                Utils.RandomSleep(1000, 4000);

                foreach (var suggestion in suggestedFriends.GetSuggestedUsers
                    .Where(s => !s.User.Following && !s.User.Status.OutComingRequest))
                {
                    if (user.LiveSettings.Follow.IsLimit)
                        break;

                    var suggestionUser = suggestion.User;

                    // Is this user allow?
                    if (Settings.Advanced.Live.SuggestedFriends.UseAllowUsersList)
                        if (!AllowUsersList.Contains(suggestionUser.Username))
                            continue;

                    if (Settings.Advanced.Live.SuggestedFriends.UseExploringProfilesWithChaining)
                    {
                        if (Utils.UseIt())
                        {
                            Profile.Explore(user, suggestionUser, true);
                        }

                        continue;
                    }

                    // Explore profiles
                    if (!exploreProfilesLimit && user.Activity.Actions.ExploreProfiles
                        >= user.LiveSettings.ExploreProfiles.Limit)
                    {
                        user.Log("Exploring profiles are limited.");
                        exploreProfilesLimit = true;
                    }

                    if (!exploreProfilesLimit && Utils.UseIt(3))
                    {
                        Profile.Explore(user, suggestionUser);
                        user.Activity.Actions.ExploreProfiles++;
                        continue;
                    }

                    bool toFollow = (!suggestionUser.IsVerified
                                        ? Settings.Advanced.Live.FollowSettings.IsVerifiedMoreImportant
                                            ? Utils.UseIt(3)
                                            : Utils.UseIt(2)
                                        : Utils.UseIt(2)) || Settings.Advanced.Live.SuggestedFriends.FollowAllFriendsSuggestions;

                    if (toFollow)
                    {
                        Follow.Do(user, suggestionUser);
                    }
                }

                // Marks list that u seen already
                if (!isMarkedSeen && Utils.UseIt(3))
                {
                    user.Do(() => user.Discover.MarksUSeen());
                    isMarkedSeen = true;
                }

                // Load More
                suggestedFriends = user.Do(() => GetSuggestions(user, suggestedFriends.MaxId));

                if (!suggestedFriends.IsOk())
                {
                    string errorMessage = suggestedFriends.IsMessage()
                ? suggestedFriends.Message
                : "GetSuggestions method exception.";

                    Log.Write(errorMessage, LogResource.SuggestedFriend);
                    throw new SomethingWrongException(errorMessage);
                }

                user.Log($"Loading more suggested friends...");

            } while (suggestedFriends.MoreAvailable);

            if (Utils.UseIt())
            {
                user.Log("Updating home page.");
                user.Do(() => user.LiveAction.ShowProfileScreen());
            }

            if (Utils.UseIt())
            {
                user.Log("Updating feed timeline.");
                user.Do(() => user.LiveAction.UpdateFeedScreen());
            }
        }

        private static SuggestedUsersResponse GetSuggestions(Instagram.Instagram user, string maxId = null)
        {
            var suggestedUsersResponse = user.Do(() => user.Discover.GetSuggestedUsers(maxId: maxId));
            string maxIdForShowMany = suggestedUsersResponse.MaxId.Replace(" ", "").Replace("[", "").Replace("]", "");

            Thread.Sleep(500);

            var showManyResponse = user.Do(() => user.FriendShips.ShowMany(maxIdForShowMany));

            foreach (var suggestion in suggestedUsersResponse.GetSuggestedUsers)
            {
                suggestion.User.Status = showManyResponse.Users.Single(s => s.Key == suggestion.User.Pk).Value;
            }

            return suggestedUsersResponse;
        }

        private static void SaveSuggestionUsers(List<Suggestion> suggestions)
        {
            lock (LockSavingSuggestionUsers)
            {
                try
                {
                    using (TextWriter textWriter = new StreamWriter("suggestions.txt", true))
                    {
                        foreach (var suggestion in suggestions)
                        {
                            textWriter.WriteLine($"{suggestion.User.Username}");
                        }
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception();
                }
            }
        }
    }
}
