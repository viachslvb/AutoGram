using System;
using System.Collections.Generic;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;

namespace AutoGram.Instagram.Request
{
    class LiveAction : RequestCollection
    {
        private bool _isFirstLoadingProfileScreen = true;

        public LiveAction(Instagram instagram) : base(instagram)
        {
        }

        public UserResponse ShowProfileScreen()
        {
            UserResponse userResponse = new UserResponse();
            User.Do(() => User.Feed.GetUserFeedStory());
            User.Do(() => User.Internal.GraphQl(friendlyName: "IGFBPayExperienceEnabled", docId: "3801135729903457", "{}"));
            User.Do(() => User.Internal.GraphQl(friendlyName: "IgDonationsEligibilityQuery", docId: "2615360401861024", "{}"));
            User.Do(() => User.Feed.GetUserFeed());
            User.Do(() => User.Internal.HighLightsTray());
            User.Do(() => userResponse = User.Account.GetUserInfo(fromModule: "self_profile"));
            User.Do(() => User.Internal.ProfileArchiveBadge());
            User.Do(() => User.Internal.FundRaiser());
            User.Do(() => User.Discover.GetSuggestedUsers(module: "self_profile"));

            User.UserInfo = userResponse;

            return userResponse;
        }

        public TraitResponse ShowSettingsScreen()
        {
            return User.Internal.GetPresenceDisabled();
        }

        public UserResponse ShowEditProfileScreen()
        {
            User.Do(() => User.Internal.SetContactPointPrefill(isLoggedUsage: true));
            User.Do(() => User.Internal.ReadMsisdnHeader(isLoggedUsage: true));

            return User.Account.GetCurrentUser();
        }

        public TraitResponse UpdateFeedScreen()
        {
            var feedTimeline = User.Timeline.GetTimelineFeed();
            Utils.RandomSleep(300, 800);

            User.Internal.ReelsTray();
            Utils.RandomSleep(300, 800);

            if (feedTimeline.IsOk() && feedTimeline.HasResults)
            {
                string latestPostId = feedTimeline.GetFirstItem.GetId;

                return User.Timeline.GetTimelineFeed(latestStoryPk: latestPostId, seenPosts: latestPostId,
                    feedViewInfoEnable: false, nextMaxId: feedTimeline.GetMaxId,
                    reason: "pagination", unseenPostsEnable: false);
            }

            return feedTimeline;
        }
    }
}
