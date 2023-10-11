using System.Runtime.CompilerServices;
using AutoGram.Helpers;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Stories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoGram.Instagram.Request
{
    class Highlights : RequestCollection
    {
        public Highlights(Instagram instagram) : base(instagram)
        {
        }

        public TraitResponse CreateHighlight(string mediaId, string title)
        {
            var cover = new JObject
            {
                {"media_id", mediaId},
                {"crop_rect", new JArray { 0.0, 0.19545822, 1.0, 0.8037307 }.ToString(Formatting.None) }
            }.ToString(Formatting.None);

            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    supported_capabilities_new = "[{\"name\":\"SUPPORTED_SDK_VERSIONS\",\"value\":\"9.0,10.0,11.0,12.0,13.0,14.0,15.0,16.0,17.0,18.0,19.0,20.0,21.0,22.0,23.0,24.0\"},{\"name\":\"FACE_TRACKER_VERSION\",\"value\":\"10\"},{\"name\":\"segmentation\",\"value\":\"segmentation_enabled\"},{\"name\":\"COMPRESSION\",\"value\":\"ETC2_COMPRESSION\"},{\"name\":\"COMPRESSION\",\"value\":\"PVR_COMPRESSION\"},{\"name\":\"WORLD_TRACKER\",\"value\":\"WORLD_TRACKER_ENABLED\"}]",
                    source = "self_profile",
                    creation_id = Utils.GenerateUploadId(),
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    cover = cover,
                    title = title,
                    media_ids = $"[{ExtensionHelper.EncodeList(new[] {mediaId})}]"
                })
                .Post("https://i.instagram.com/api/v1/highlights/create_reel/")
                .ToResponse<TraitResponse>();
        }

        public HighlightsResponse GetHighlightMedias(string userId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    source = "follow_list",
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    user_ids = new JArray(userId)
                })
                .Post("https://i.instagram.com/api/v1/feed/reels_media/")
                .ToResponse<HighlightsResponse>();
        }

        public TraitResponse SeenBroadcast(UserStory story)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    container_module = "follow_list",
                    live_vods_skipped = new JObject(),
                    nuxes_skipped = new JObject(),
                    nuxes = new JObject(),
                    reels = new JObject
                    {
                        {$"{story.Id}_{story.User.Pk}", new JArray($"{story.TakenAt}_{Utils.DateTimeNowTotalSeconds}")}
                    },
                    live_vods = new JObject(),
                    reel_media_skipped = new JObject()
                })
                .Post("https://i.instagram.com/api/v2/media/seen/?reel=1&live_vod=0")
                .ToResponse<TraitResponse>();
        }
    }
}