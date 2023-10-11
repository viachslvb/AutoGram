using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class ThreadResponse : TraitResponse, IResponse
    {
        [JsonProperty("thread")] public ThreadModel Thread;
    }
}
