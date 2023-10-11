using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class InboxModel
    {
        [JsonProperty("threads")] public List<ThreadModel> Threads;

        [JsonProperty("unseen_count")] public int UnseenCount;

        [JsonProperty("oldest_cursor")] public string OldestCursor;

        [JsonProperty("has_older")] public bool HasOlder;
    }
}
