using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Model
{
    class MediaItem
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("pk")]
        public string Pk;

        [JsonProperty("code")]
        public string Code;

        [JsonProperty("taken_at")]
        public long TakenAt;

        [JsonProperty("can_view_more_preview_comments")]
        public bool CanViewMorePreviewComments;

        [JsonProperty("has_more_comments")]
        public bool HasMoreComments;

        [JsonProperty("preview_comments")]
        public List<MediaComment> PreviewComments;

        [JsonProperty("user")]
        public User User;

        [JsonProperty("comment_count")]
        public int CommentCount;

        [JsonProperty("like_count")]
        public int LikeCount;

        [JsonProperty("caption")] public Caption Caption;

        [JsonProperty("media_type")] public int MediaType;
    }
}
