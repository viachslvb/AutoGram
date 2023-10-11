using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class UserFeedResponse : TraitResponse, IResponse
    {
        [JsonProperty("items")]
        public MediaItem[] MediaItems;

        [JsonProperty("num_results")]
        public int NumResults;

        [JsonProperty("more_available")]
        public bool MoreAvailable;

        [JsonProperty("next_max_id")]
        public string NextMaxId;
    }
}
