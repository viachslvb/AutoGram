using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class VisualMediaModel
    {
        [JsonProperty("media")] public MediaItem Media;
        [JsonProperty("seen_count")] public int SeenCount;
        [JsonProperty("view_mode")] public string ViewMode;
    }
}
