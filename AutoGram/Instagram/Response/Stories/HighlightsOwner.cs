using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Stories
{
    class HighlightsOwner
    {
        [JsonProperty("id")] public long Id;
        [JsonProperty("latest_reel_media")] public long LatestReelMedia;
        [JsonProperty("seen")] public int Seen;
        [JsonProperty("user")] public User User;
        [JsonProperty("items")] public List<UserStory> Stories;
        [JsonProperty("media_count")] public int MediaCount;
    }
}
