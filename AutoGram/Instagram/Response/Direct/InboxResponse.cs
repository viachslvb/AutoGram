using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class InboxResponse
    {
        [JsonProperty("inbox")] public InboxModel Inbox;

        [JsonProperty("pending_requests_total")] public int PendingRequestsTotal;
    }
}
