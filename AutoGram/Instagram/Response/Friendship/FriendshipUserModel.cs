using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Friendship
{
    class FriendshipUserModel : User
    {
        [JsonProperty("latest_reel_media")] public string LatestReelMedia;

        public bool IsStories => !string.IsNullOrEmpty(LatestReelMedia) && LatestReelMedia != "0";
    }
}
