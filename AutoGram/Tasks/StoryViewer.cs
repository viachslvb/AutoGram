using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Database;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response.Model;
using AutoGram.Services;
using AutoGram.Task.SubTask;

namespace AutoGram.Task
{
    static class StoryViewer
    {
        private static readonly List<string> UsernameList;

        private static int _usernameCounter;

        private static readonly object DataLocker = new object();

        static StoryViewer()
        {
            var usernameSource = Settings.Advanced.StoryViewer.UsernameSource;
            UsernameList = usernameSource.Split(' ').ToList();
            UsernameList.Shuffle();
        }

        public static void Do(Instagram.Instagram user)
        {
            int seenErrors = 0;
            int seenCounter = 0;

            if (!Settings.Advanced.StoryViewer.UseLikersForViewingStories)
            {
                while (true)
                {
                    // pk
                    string username = GetUsername();

                    var targetUser = new User { Pk = username };
                    var profileResponse = Profile.Open(user, targetUser);

                    var targetUsername = profileResponse.UserInfo.User.Username;

                    user.Log($"Opening profile {targetUsername}");

                    string nextMaxId = string.Empty;
                    string rankToken = Utils.GenerateUUID(true);

                    int seenChainCounter = 0;

                    while (true)
                    {
                        user.Log($"Loading followers of {targetUsername}");

                        // Load followers
                        var friendshipsResponse = user.Do(() =>
                            user.FriendShips.GetFollowers(profileResponse.UserInfo.User.Pk, rankToken, nextMaxId));

                        // Load friendships statuses
                        string userIds = friendshipsResponse.Users.Select(u => u.Pk).Aggregate((x, y) => $"{x},{y}");
                        user.Do(() => user.FriendShips.ShowMany(userIds));

                        var targetFriendships = friendshipsResponse.Users.Where(u => u.IsStories).ToList();

                        user.Log($"Founded {targetFriendships.Count} users with stories.");

                        foreach (var targetFriendship in targetFriendships)
                        {
                            if (seenCounter >= Settings.Advanced.StoryViewer.Count)
                            {
                                throw new SuspendTaskException();
                            }

                            if (SeenStoriesRepository.AlreadyExists(targetFriendship.Pk))
                            {
                                if(seenChainCounter >= 15) break;

                                seenChainCounter++;
                                continue;
                            }
                            seenChainCounter = 0;

                            var sendTextResponse = user.Do(() =>
                                user.Direct.SendTextToRecipientUsers("some data", targetFriendship.Pk));

                            if (sendTextResponse.IsOk())
                            {
                                var seenStory = new SeenStory()
                                {
                                    OwnerPk = targetFriendship.Pk,
                                    OwnerUsername = targetFriendship.Username,
                                    OwnerFullname = targetFriendship.Full_name,
                                    SourceType = 0,
                                    Source = targetUsername,
                                    StoryPk = null
                                };
                                SeenStoriesRepository.Create(seenStory);

                                user.Storage.SeenStories++;
                                user.Log($"Send message to {targetFriendship.Username}.");

                                seenCounter++;
                                seenErrors = 0;

                                user.Log($"Sleep {Settings.Advanced.StoryViewer.PauseMilliseconds} ms.");
                                Thread.Sleep(Settings.Advanced.StoryViewer.PauseMilliseconds);
                            }
                            else
                            {
                                if (seenErrors > 2)
                                {
                                    user.Log($"Send message error. {sendTextResponse.Message}");
                                    throw new SuspendTaskException();
                                }

                                seenErrors++;
                            }
                        }

                        if (friendshipsResponse.IsNextMaxId)
                        {
                            nextMaxId = friendshipsResponse.NextMaxId;
                        }
                        else break;

                        if (seenChainCounter >= 15) break;
                    }
                }
            }
            else
            {
                while (true)
                {
                    string username = GetUsername();

                    var targetUser = new User { Pk = username };
                    var profileResponse = Profile.Open(user, targetUser);

                    var targetUsername = profileResponse.UserInfo.User.Username;

                    user.Log($"Opening profile {targetUsername}");

                    if (profileResponse.UserFeed.MediaItems.Any())
                    {
                        var lastFeedItem = profileResponse.UserFeed.MediaItems.FirstOrDefault();

                        var likersResponse = user.Do(() => user.Media.GetLikers(lastFeedItem.Id));

                        // Load friendships statuses
                        string userIds = likersResponse.Users.Select(u => u.Pk).Aggregate((x, y) => $"{x},{y}");
                        user.Do(() => user.FriendShips.ShowMany(userIds));

                        var targetFriendships = likersResponse.Users.Where(u => u.IsStories).ToList();

                        user.Log($"Founded {targetFriendships.Count} users with stories.");

                        foreach (var targetFriendship in targetFriendships)
                        {
                            if (seenCounter >= Settings.Advanced.StoryViewer.Count)
                            {
                                throw new SuspendTaskException();
                            }

                            if (SeenStoriesRepository.AlreadyExists(targetFriendship.Pk))
                                continue;

                            var storiesResponse = user.Do(() => user.Highlights.GetHighlightMedias(targetFriendship.Pk));

                            if (!storiesResponse.IsValid) continue;

                            var latestStory = storiesResponse.GetLatestStory();

                            var seenResponse = user.Do(() => user.Highlights.SeenBroadcast(latestStory));

                            if (seenResponse.IsOk())
                            {
                                var seenStory = new SeenStory()
                                {
                                    OwnerPk = targetFriendship.Pk,
                                    OwnerUsername = targetFriendship.Username,
                                    OwnerFullname = targetFriendship.Full_name,
                                    SourceType = 0,
                                    Source = targetUsername,
                                    StoryPk = latestStory.Id
                                };
                                SeenStoriesRepository.Create(seenStory);

                                user.Storage.SeenStories++;
                                user.Log($"Seen {targetFriendship.Username} stories.");

                                seenCounter++;
                                seenErrors = 0;

                                user.Log($"Sleep {Settings.Advanced.StoryViewer.PauseMilliseconds} ms.");
                                Thread.Sleep(Settings.Advanced.StoryViewer.PauseMilliseconds);
                            }
                            else
                            {
                                if (seenErrors > 2)
                                {
                                    user.Log($"Storyviewing error. {seenResponse.Message}");
                                    throw new SuspendTaskException();
                                }

                                seenErrors++;
                            }
                        }
                    }
                    else
                    {
                        user.Log($"Media not found.");
                        throw new SuspendTaskException();
                    }
                }
            }
        }

        private static string GetUsername()
        {
            lock (DataLocker)
            {
                if (UsernameList.Count > _usernameCounter)
                    return UsernameList[_usernameCounter++];

                _usernameCounter = 0;
                return UsernameList[_usernameCounter++];
            }
        }

    }
}
