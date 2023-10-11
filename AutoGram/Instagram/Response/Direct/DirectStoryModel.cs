using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class DirectStoryModel
    {
        [JsonProperty("items")] public List<ThreadItemModel> Items;
        [JsonProperty("last_activity_at")] public long LastActivityAt;
        [JsonProperty("unseen_count")] public int UnseenCount;
        [JsonProperty("newest_cursor")] public string NewestCursor;

    }
}
