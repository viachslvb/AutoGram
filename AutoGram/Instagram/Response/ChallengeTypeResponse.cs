using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AutoGram.Instagram.Response
{
    class ChallengeTypeResponse : TraitResponse, IResponse
    {
        public string step_name;

        [JsonProperty("user_id")] public string UserId;

        [JsonProperty("nonce_code")] public string NonceCode;

        // Automated Behavior Challenge Params

        [JsonProperty("bloks_action")] public string BlocksAction;
        [JsonProperty("cni")] public string CniValue;

        public bool IsEmailChallenge() => this.step_name == "select_verify_method";
        public bool IsPhoneChallenge() => this.step_name == "submit_phone";
        public bool IsAutomatedBehavior() => this.step_name == "scraping_warning";
        public bool IsDeletedContentChallenge() => this.step_name == "deleted_content_informational";
        public bool IsUndefinedChallenge() => string.IsNullOrEmpty(step_name);
    }


}
