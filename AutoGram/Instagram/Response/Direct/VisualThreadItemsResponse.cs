using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class VisualThreadItemsResponse : TraitResponse, IResponse
    {
        [JsonProperty("items")] public List<ThreadItemModel> Items;
        [JsonProperty("thread_id")] public string ThreadId;
        [JsonProperty("unseen_count")] public int UnseenCount;
        [JsonProperty("newest_cursor")] public string NewestCursor;
    }
}
