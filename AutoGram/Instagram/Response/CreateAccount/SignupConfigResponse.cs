using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.CreateAccount
{
    class SignupConfigResponse : TraitResponse, IResponse
    {
        [JsonProperty("age_required")] public bool IsAgeRequired { get; set; }
        [JsonProperty("gdpr_required")] public bool IsGdprRequired { get; set; }
        [JsonProperty("tos_acceptance_not_required")] public bool IsTosAcceptanceNotRequired { get; set; }
        [JsonProperty("tos_version")] public string TosVersion { get; set; }
    }
}
