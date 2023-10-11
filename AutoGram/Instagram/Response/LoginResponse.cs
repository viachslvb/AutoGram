using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class LoginResponse : TraitResponse, IResponse
    {
        [JsonProperty("logged_in_user")]
        public Model.User User;

        [JsonProperty("invalid_credentials")]
        public string InvalidCredentials;

        public bool IsInvalidCredentials()
        {
            return this.InvalidCredentials != null;
        }
    }

    class LoginButtonModel
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("action")]
        public string Action;

        [JsonProperty("stop_deletion_token")]
        public string StopDeletionToken;
    }
}
