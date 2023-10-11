
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;

namespace AutoGram.Instagram.Request
{
    class Feed : RequestCollection
    {
        public Feed(Instagram instagram) : base(instagram)
        {
        }

        public UserFeedResponse GetUserFeed(string userId = null, string maxId = null)
        {
            if (userId == null) userId = User.AccountId;

            User.Request.AddUrlParam("exclude_comment", "true");

            if (!string.IsNullOrEmpty(maxId))
            {
                User.Request.AddUrlParam("max_id", maxId);
            }

            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("only_fetch_first_carousel_media", "false")
                .Get($"https://i.instagram.com/api/v1/feed/user/{userId}/")
                .ToResponse<UserFeedResponse>();
        }

        public TraitResponse GetUserFeedStory(string userId = null, string maxId = null)
        {
            if (userId == null) userId = User.AccountId;

            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("supported_capabilities_new", "[{\"name\":\"SUPPORTED_SDK_VERSIONS\",\"value\":\"103.0,104.0,105.0,106.0,107.0,108.0,109.0,110.0,111.0,112.0,113.0,114.0,115.0,116.0,117.0,118.0,119.0,120.0,121.0\"},{\"name\":\"FACE_TRACKER_VERSION\",\"value\":\"14\"},{\"name\":\"segmentation\",\"value\":\"segmentation_enabled\"},{\"name\":\"COMPRESSION\",\"value\":\"ETC2_COMPRESSION\"},{\"name\":\"world_tracker\",\"value\":\"world_tracker_enabled\"},{\"name\":\"gyroscope\",\"value\":\"gyroscope_enabled\"}]")
                .Get($"https://i.instagram.com/api/v1/feed/user/{userId}/story/")
                .ToResponse<TraitResponse>();
        }
    }
}
