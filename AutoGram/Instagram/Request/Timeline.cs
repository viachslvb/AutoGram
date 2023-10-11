using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Model;

namespace AutoGram.Instagram.Request
{
    class Timeline : RequestCollection
    {
        public Timeline(Instagram instagram) : base(instagram)
        {
        }

        public FeedTimelineResponse GetTimelineFeed(string seenPosts = "", string latestStoryPk = "",
            bool feedViewInfoEnable = true, string nextMaxId = "", string reason = "pull_to_refresh",
            bool unseenPostsEnable = true, bool recoveryFromCrash = false)
        {
            int isPullToRefresh = reason == "pull_to_refresh" ? 1 : 0;

            User.Request
                .AddCustomHeader("X-Ads-Opt-Out", "0")
                .AddCustomHeader("X-Google-AD-ID", User.AdvertisingId)
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddCustomHeader("X-CM-Bandwidth-KBPS", "-1.000")
                .AddCustomHeader("X-CM-Latency", "-1.000")
                .AddDefaultHeaders();

            if (feedViewInfoEnable)
            {
                User.Request
                    .AddParam("feed_view_info", "[]");
            }

            if (latestStoryPk != "")
            {
                User.Request
                    .AddParam("latest_story_pk", latestStoryPk);
            }

            if (seenPosts != "")
            {
                User.Request.AddParam("seen_posts", seenPosts);
            }

            User.Request
                .AddParam("phone_id", User.PhoneId);

            if (nextMaxId != "")
            {
                User.Request
                    .AddParam("max_id", nextMaxId);
            }

            User.Request
                .AddParam("reason", reason)
                .AddParam("battery_level", User.BatteryLevel)
                .AddParam("timezone_offset", User.TimezoneOffset)
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("device_id", User.Uuid)
                .AddParam("is_pull_to_refresh", isPullToRefresh)
                .AddParam("_uuid", User.Uuid);

            if (unseenPostsEnable)
            {
                User.Request
                    .AddParam("unseen_posts", "");
            }

            User.Request
                .AddParam("is_charging", "1");

            if (recoveryFromCrash)
            {
                User.Request
                    .AddParam("recovered_from_crash", "1");
            }

            return User.Request
                .AddParam("will_sound_on", "0")
                .AddParam("session_id", User.SessionId)
                .AddParam("bloks_versioning_id", User.App.BloksVersionId)
                .Post("https://i.instagram.com/api/v1/feed/timeline/")
                .ToResponse<FeedTimelineResponse>();
        }

        public UserStoriesResponse GetUserStory(string userId = null)
        {
            if (userId == null) userId = User.AccountId;

            return User.Request
                .AddDefaultHeaders()
                .AddUrlParamDecode("supported_capabilities_new", Constants.SupportedCapabilities)
                .Get($"https://i.instagram.com/api/v1/feed/user/{userId}/story/")
                .ToResponse<UserStoriesResponse>();
        }

        public MediaConfigureResponse UploadVideo(MediaObject mediaObject)
        {
            var response = User.Do(() => User.Internal.UploadVideo(mediaObject));

            if (response.IsOk())
            {
                User.Do(() => User.Timeline.GetTimelineFeed());
                User.Do(() => User.Internal.ReelsTray());
            }

            return response;
        }

        public MediaConfigureResponse UploadPhoto(MediaObject mediaObject)
        {
            var response = User.Do(() => User.Internal.UploadPhoto(mediaObject));

            if (response.IsOk())
            {
                User.Do(() => User.Timeline.GetTimelineFeed());
                User.Do(() => User.Internal.ReelsTray());
            }

            return response;
        }

        public MediaConfigureResponse UploadAlbum(List<MediaObject> mediaObjects)
        {
            var response = User.Do(() => User.Internal.UploadAlbum(mediaObjects));

            if (response.IsOk())
            {
                User.Do(() => User.Timeline.GetTimelineFeed());
                User.Do(() => User.Internal.ReelsTray());
            }

            return response;
        }
    }
}
