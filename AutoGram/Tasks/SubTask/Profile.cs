using System;
using System.Collections.Generic;
using System.Linq;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Model;

namespace AutoGram.Task.SubTask
{
    class Profile
    {
        public static void Explore(Instagram.Instagram user, SuggestedUser targetUser, bool followChainingUsersFromRandomCommentOwner = false)
        {
            // Open Profile
            user.Log($"Opening profile {targetUser.Username}.");

            var profile = Open(user, targetUser);

            Utils.RandomSleep(2000, 4000);

            int viewingDepth = 0;
            bool isCommentChainingUserUsed = false;

            do
            {
                if (profile.UserInfo.User.IsPrivate)
                    break;

                if (!isCommentChainingUserUsed && followChainingUsersFromRandomCommentOwner)
                {
                    if (profile.UserFeed.MediaItems.Any())
                    {
                        var feedItemsWithComments = profile.UserFeed.MediaItems.Where(s => s.HasMoreComments).ToList();
                        if (feedItemsWithComments.Count < 2)
                            break;

                        var feedItem = feedItemsWithComments[Utils.Random.Next(feedItemsWithComments.Count)];

                        Utils.RandomSleep(2000, 4000);

                        // Like media
                        if (Utils.UseIt(3))
                        {
                            Like.Media(user, targetUser, feedItem.Id, "photo_view_profile");
                        }

                        Utils.RandomSleep(1000, 3000);

                        // Open Comments
                        var comments = user.Do(() => user.Media.GetComments(feedItem.Id)).Comments;
                        user.Log($"Opening comments.");

                        Utils.RandomSleep(1000, 4000);

                        if (comments.Any())
                        {
                            var randomComment = comments[Utils.Random.Next(comments.Count)];

                            // Like comment
                            if (Utils.UseIt(3))
                            {
                                Like.Comment(user, randomComment.Pk);
                            }

                            Utils.RandomSleep(2000, 3500);

                            // Open profile
                            var profileFromComments = Open(user, randomComment.User, "comment_owner");
                            user.Log($"Opening profile {profileFromComments.UserInfo.User.Username}.");

                            Utils.RandomSleep(2000, 3500);

                            // Load posts
                            if (Utils.UseIt())
                            {
                                user.Log($"Loading more posts of {profileFromComments.UserInfo.User.Username}.");

                                if (profileFromComments.UserFeed.MoreAvailable)
                                {
                                    user.Do(
                                        () =>
                                            user.Feed.GetUserFeed(profileFromComments.UserInfo.User.Pk,
                                                profileFromComments.UserFeed.NextMaxId));
                                    Utils.RandomSleep(1000, 2500);
                                }
                            }

                            // Get discover chaining by target user

                            var chainingResponse =
                                user.Do(
                                        () => user.Discover.
                                            Chaining(profileFromComments.UserInfo.User.Pk));

                            if (!chainingResponse.IsOk())
                            {
                                if (chainingResponse.IsMessage()
                                    && chainingResponse.GetMessage().Contains("Not eligible for chaining"))
                                {
                                    throw new SomethingWrongException("User does not eligible for chaining.");
                                }
                            }

                            user.Log($"Load chaining from {profileFromComments.UserInfo.User.Username}");

                            isCommentChainingUserUsed = true;

                            int chainingMax = 15;
                            int chainingCounter = 0;

                            foreach (var chainingUser in chainingResponse.Users)
                            {
                                if (chainingUser.IsPrivate)
                                    continue;

                                // Follow user
                                if (Utils.UseIt(3))
                                {
                                    Follow.Do(user, chainingUser);
                                    chainingCounter++;
                                }

                                if (user.LiveSettings.Follow.IsLimit)
                                    break;

                                if (chainingCounter >= chainingMax)
                                    break;
                            }
                        }
                    }
                }

                Utils.RandomSleep(1600, 4500);

                if (Utils.UseIt(4))
                {
                    var randomFeedItem = profile.UserFeed.MediaItems[Utils.Random.Next(profile.UserFeed.MediaItems.Length)];

                    Utils.RandomSleep(2000, 4000);

                    // Like media
                    if (Utils.UseIt(3))
                    {
                        Like.Media(user, targetUser, randomFeedItem.Id, "photo_view_profile");
                    }

                    Utils.RandomSleep(1000, 3000);
                }

                if (Utils.UseIt(3))
                    break;

                if (viewingDepth
                    < Settings.Advanced.Live.ExploreProfilesSettings.ViewingDepthLimit
                    && profile.UserFeed.MoreAvailable)
                {
                    user.Log($"Loading more posts of {targetUser.Username}.");

                    profile.UserFeed = user.Do(() => user.Feed.GetUserFeed(targetUser.Pk, profile.UserFeed.NextMaxId));
                    viewingDepth++;

                    Utils.RandomSleep(1000, 2000);
                }
                else break;

            } while (profile.UserFeed.MoreAvailable);

            // Limit ? return
            if (user.LiveSettings.Follow.IsLimit)
                return;

            // !Limit, UseIt? Follow this user
            if (Utils.UseIt(6)
                && !targetUser.Status.Following
                && !targetUser.Status.OutComingRequest)
            {
                Utils.RandomSleep(500, 2000);
                Follow.Do(user, targetUser);
            }
        }

        public static ProfileResponse Open(Instagram.Instagram user, User targetUser, string fromModule = null)
        {
            ProfileResponse profileResponse = new ProfileResponse();

            user.Do(() => profileResponse.UserInfo = user.Account.GetUserInfo(targetUser.Pk, fromModule));

            if (profileResponse?.UserInfo?.User?.Follower_count == null)
            {
                throw new OpenProfileException();
            }

            if (profileResponse.UserInfo.User.Follower_count == 0)
            {
                throw new OpenProfileException();

                user.Log($"User {profileResponse.UserInfo.User.Username} marked as 18+ only.");
                user.Do(() => user.FriendShips.MarkUserOverage(targetUser.Pk));
                user.Do(() => profileResponse.UserInfo = user.Account.GetUserInfo(targetUser.Pk, fromModule));
            }

            user.Do(() => profileResponse.UserFeed = user.Feed.GetUserFeed(targetUser.Pk));

            return profileResponse;
        }

        public class ProfileResponse
        {
            public UserFeedResponse UserFeed { get; set; }
            public UserResponse UserInfo { get; set; }

            public UserStoriesResponse UserStories { get; set; }
        }
    }
}
