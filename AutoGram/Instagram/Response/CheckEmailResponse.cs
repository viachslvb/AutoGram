using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class CheckEmailResponse : TraitResponse, IResponse
    {
        [JsonProperty("gdpr_required")]
        public bool IsGdprRequired { get; set; }
    }
}
