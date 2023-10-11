using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Stories
{
    class UserStory
    {
        [JsonProperty("taken_at")] public long TakenAt;
        [JsonProperty("pk")] public long Pk;
        [JsonProperty("id")] public string Id;
        [JsonProperty("media_type")] public int MediaType;
        [JsonProperty("code")] public string Code;
        [JsonProperty("user")] public User User;
        [JsonProperty("is_reel_media")] public bool IsReelMedia;
    }
}
