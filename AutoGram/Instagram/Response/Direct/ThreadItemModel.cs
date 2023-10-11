using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class ThreadItemModel
    {
        [JsonProperty("item_id")] public string ItemId;

        [JsonProperty("user_id")] public string UserId;

        [JsonProperty("timestamp")] public long Timestamp;

        [JsonProperty("item_type")] public string ItemType;

        [JsonProperty("text")] public string Text;

        [JsonProperty("raven_media")] public MediaItem RavenMedia;

        [JsonProperty("client_context")] public string ClientContext;

        [JsonProperty("visual_media")] public VisualMediaModel VisualMedia;

        [JsonProperty("link")] public DirectLinkItem Link;

        [JsonIgnore] public bool IsLink => Link != null;

        public bool IsRavenMedia() => ItemType == "raven_media";
        public bool IsReelShare() => ItemType == "reel_share";
        public bool IsMedia() => ItemType == "media";
        public bool IsUnseenDirectStory()
        {
            if (!IsRavenMedia()) return false;
            return VisualMedia?.SeenCount == 0;
        }
    }
}
