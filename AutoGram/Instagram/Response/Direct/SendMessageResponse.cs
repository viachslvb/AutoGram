using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class SendMessageResponse : TraitResponse, IResponse
    {
        [JsonProperty("action")] public string Action;

        [JsonProperty("status_code")] public string StatusCode;

        [JsonProperty("payload")] public PayloadModel Payload;

        public bool IsForbidden => StatusCode == "403";
    }
}
