using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class PayloadModel
    {
        [JsonProperty("client_context")] public string ClientContext;
        [JsonProperty("item_id")] public string ItemId;
        [JsonProperty("timestamp")] public string Timestamp;
        [JsonProperty("thread_id")] public string ThreadId;
        [JsonProperty("message")] public string Message;
    }
}
