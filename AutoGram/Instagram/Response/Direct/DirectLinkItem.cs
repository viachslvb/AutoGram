using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class DirectLinkItem
    {
        [JsonProperty("text")] public string Text;
        [JsonProperty("link_context")] public DirectLinkContextItem LinkContext;

        [JsonIgnore]
        public bool IsSpam => LinkContext != null
                              && LinkContext.LinkSummary == ""
                              && LinkContext.LinkImageUrl == ""
                              && LinkContext.LinkTitle == "";
    }

    class DirectLinkContextItem
    {
        [JsonProperty("link_url")] public string LinkUrl;
        [JsonProperty("link_title")] public string LinkTitle;
        [JsonProperty("link_summary")] public string LinkSummary;
        [JsonProperty("link_image_url")] public string LinkImageUrl;
    }
}
