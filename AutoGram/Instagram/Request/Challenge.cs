using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Task.SubTask;

namespace AutoGram.Instagram.Request
{
    class Challenge : RequestCollection
    {
        private readonly Response.Model.Challenge _challenge;

        public Challenge(Instagram instagram, Response.Model.Challenge challenge) : base(instagram)
        {
            _challenge = challenge;
        }

        public ChallengeTypeResponse Type =>
            User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1{_challenge.GetUrl()}")
                .ToResponse<ChallengeTypeResponse>();

        public void SendVerificationCodeToEmail()
        {
            User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    choice = "1",
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId
                })
                .Post($"https://i.instagram.com/api/v1{_challenge.GetUrl()}");
        }

        public TraitResponse ApproveDeletedContent()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    choice = "0",
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId
                })
                .Post($"https://i.instagram.com/api/v1{_challenge.GetUrl()}")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse CompleteAutomatedBehaviorChallenge(string cni)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    should_promote_account_status = 0,
                    _uuid = User.Uuid,
                    bk_client_context = $"{{\"bloks_version\":\"{User.App.BloksVersionId}\",\"styles_id\":\"instagram\"}}",
                    challenge_context = $"{{\"step_name\": \"scraping_warning\", \"cni\": {cni}, \"is_stateless\": false, \"challenge_type_enum\": \"UNKNOWN\", \"present_as_modal\": false}}",
                    bloks_versioning_id = User.App.BloksVersionId
                })
                .Post("https://b.i.instagram.com/api/v1/bloks/apps/com.instagram.challenge.navigation.take_challenge/")
                .ToResponse<TraitResponse>();
        }

        public LoginResponse VerifyEmail(string code)
        {
            var loginResponse = User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    security_code = code,
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId
                })
                .Post($"https://i.instagram.com/api/v1{_challenge.GetUrl()}")
                .ToResponse<LoginResponse>();

            if (!loginResponse.IsOk()) return loginResponse;

            User.AccountId = loginResponse.User.Pk;
            User.RankToken = User.AccountId + "_" + User.Uuid;

            User.SendLoginFlow();

            return loginResponse;
        }

        public TraitResponse SendPhoneVerificationCode(string userId, string nonceCode, string phoneNumber)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_number = phoneNumber,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/challenge/{userId}/{nonceCode}/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse VerifyPhoneVerificationCode(string userId, string nonceCode, string code)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    security_code = code,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/challenge/{userId}/{nonceCode}/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ResetVerificationPhone(string userId, string nonceCode)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/challenge/reset/{userId}/{nonceCode}/")
                .ToResponse<TraitResponse>();
        }
    }
}
