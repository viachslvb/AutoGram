using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using xNet;

namespace AutoGram.Instagram.Response
{
    class TraitResponse : IHttpResponse
    {
        public HttpResponse HttpResponse { get; set; }

        public string Status;
        public string Message;
        public Model.Challenge Challenge;

        [JsonProperty("checkpoint_url")]
        public string CheckPointUrl;

        [JsonProperty("error_type")]
        public string ErrorType;

        [JsonProperty("buttons")]
        public IEnumerable<LoginButtonModel> Actions;

        public bool IsOk()
        {
            return this.Status == "ok";
        }

        public bool IsMessage()
        {
            return this.Message != null;
        }

        public bool IsErrorType()
        {
            return this.ErrorType != null;
        }

        public string GetStatus()
        {
            return this.Status;
        }

        public string GetMessage()
        {
            return this.Message;
        }

        public bool IsLoginRequired()
        {
            return this.IsMessage() && this.Message.Contains("login_required");
        }

        public bool IsChallengeRequired()
        {
            return this.IsMessage() && this.Message.Contains("challenge_required")
                || this.IsMessage() && this.Message.Contains("checkpoint_required");
        }

        public bool IsPrivacyFlow()
        {
            return this.CheckPointUrl != null && this.CheckPointUrl.Contains("privacy_flow=1&next=instagram://checkpoint/dismiss");
        }

        // check before isInactiveUser !!!
        // todo: repair this
        public bool IsDeletedUser()
        {
            return IsErrorType() && ErrorType.Contains("inactive user")
                && Actions != null && Actions.Any(x => x.Action == "stop_account_deletion");
        }

        public string GetStopDeletionToken()
        {
            return Actions.FirstOrDefault(x => x.Action == "stop_account_deletion").StopDeletionToken;
        }

        public bool IsInactiveUser()
        {
            return IsErrorType() && ErrorType.Contains("inactive user");
        }

        public bool IsChallenge()
        {
            return this.Challenge != null;
        }

        public bool IsConsentRequired()
        {
            return IsMessage() && Message.Contains("consent_required");
        }

        public bool IsCheckpointUrl()
        {
            return this.CheckPointUrl != null;
        }

        
    }

    interface IHttpResponse
    {
        HttpResponse HttpResponse { get; set; }
    }
}
