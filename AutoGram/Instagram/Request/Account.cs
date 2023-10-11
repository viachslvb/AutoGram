using System.IO;
using System.Linq;
using System.Text;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.CreateAccount;
using Newtonsoft.Json;
using xNet;

namespace AutoGram.Instagram.Request
{
    class Account : RequestCollection
    {
        public Account(Instagram instagram) : base(instagram)
        {
        }

        public LoginResponse Login()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    jazoest = CreateJazoest(User.PhoneId),
                    country_codes = "[{\"country_code\":\"" + Constants.CountryCodeRegistration + "\",\"source\":[\"sim\",\"network\",\"default\",\"sim\"]}]",
                    phone_id = User.PhoneId,
                    username = User.Username,
                    adid = User.AdvertisingId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    google_tokens = "[]",
                    login_attempt_count = "0",
                    enc_password = Utils.GetEncryptedPassword(User.State.PwPubKey, User.State.PwKeyId, User.Password),
                })
                .Post("https://i.instagram.com/api/v1/accounts/login/")
                .ToResponse<LoginResponse>();
        }

        public LoginResponse RestoreAccountAndLogin(string stopDeletionToken)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    jazoest = CreateJazoest(User.PhoneId),
                    country_codes = "[{\"country_code\":\"" + Constants.CountryCodeRegistration + "\",\"source\":[\"sim\",\"network\",\"default\",\"sim\"]}]",
                    phone_id = User.PhoneId,
                    username = User.Username,
                    adid = User.AdvertisingId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    google_tokens = "[]",
                    login_attempt_count = "0",
                    enc_password = Utils.GetEncryptedPassword(User.State.PwPubKey, User.State.PwKeyId, User.Password),
                    stop_deletion_token = stopDeletionToken
                })
                .Post("https://i.instagram.com/api/v1/accounts/login/")
                .ToResponse<LoginResponse>();
        }

        static string CreateJazoest(string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            int sum = bytes.Aggregate(0, (current, t) => current + t);

            return $"2{sum}";
        }

        public UserResponse GetUserInfo(string userId = null, string fromModule = null, string entryPoint = null)
        {
            if (userId == null) userId = User.AccountId;

            User.Request
                .AddDefaultHeaders();

            if (!string.IsNullOrEmpty(fromModule))
                User.Request
                    .AddUrlParam("from_module", fromModule);

            if (!string.IsNullOrEmpty(entryPoint))
                User.Request
                    .AddUrlParam("entry_point", entryPoint);

            var response = User.Request
                .Get($"https://i.instagram.com/api/v1/users/{userId}/info/");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new UserNotFoundException();

            return response.ToResponse<UserResponse>();
        }

        public UserResponse GetCurrentUser(bool edit = false)
        {
            if (edit)
            {
                return User.Request
                    .AddDefaultHeaders()
                    .AddUrlParam("edit", "true")
                    .Get("https://i.instagram.com/api/v1/accounts/current_user/")
                    .ToResponse<UserResponse>();
            }
            else
            {
                return User.Request
                    .AddDefaultHeaders()
                    .Get("https://i.instagram.com/api/v1/accounts/current_user/")
                    .ToResponse<UserResponse>();
            }
        }

        public TraitResponse GetAccountFamily()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/multiple_accounts/get_account_family/")
                .ToResponse<UserResponse>();
        }

        public void ReportFeedback()
        {
            try
            {
                User.Request
                    .AddDefaultHeaders()
                    .AddSignedParams(new
                    {
                        _csrftoken = User.GetToken()
                    })
                    .Post("https://i.instagram.com/api/v1/repute/report_problem/instagram_signup/");
            }
            catch (System.Exception)
            {
                // return
            }
        }

        public TraitResponse SetPublic()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/set_public/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SetBiography(string desc)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _uid = User.AccountId,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid,
                    raw_text = desc
                })
                .Post("https://i.instagram.com/api/v1/accounts/set_biography/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SetPrivate()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/set_private/")
                .ToResponse<TraitResponse>();
        }

        public UserResponse EditProfile(Response.Model.User user)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _uuid = User.Uuid,
                    _uid = User.AccountId,
                    external_url = user.External_url,
                    phone_number = user.Phone_number,
                    username = user.Username,
                    first_name = user.Full_name,
                    biography = Utils.EncodeNonAsciiCharacters(user.Biography),
                    email = user.Email,
                    device_id = User.DeviceId
                }, true)
                .Post("https://i.instagram.com/api/v1/accounts/edit_profile/")
                .ToResponse<UserResponse>();
        }

        public CheckEmailResponse CheckEmail()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    android_device_id = User.DeviceId,
                    login_nonce_map = "{}",
                    _csrftoken = User.GetToken(),
                    login_nonces = "[]",
                    email = User.Email.Username,
                    qe_id = Utils.GenerateUUID(true),
                    waterfall_id = User.RegisterWaterfallId,
                })
                .Post("https://i.instagram.com/api/v1/users/check_email/")
                .ToResponse<CheckEmailResponse>();
        }

        public TraitResponse CheckPhoneNumber()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_number = User.PhoneNumber,
                    login_nonces = "[]",
                    device_id = User.DeviceId
                })
                .Post("https://i.instagram.com/api/v1/accounts/check_phone_number/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SendSignupSmsCode()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    phone_number = User.PhoneNumber,
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    waterfall_id = User.RegisterWaterfallId
                })
                .Post("https://i.instagram.com/api/v1/accounts/send_signup_sms_code/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ValidateSignupSmsCode(string verificationCode)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    verification_code = verificationCode,
                    phone_number = User.PhoneNumber,
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    waterfall_id = User.RegisterWaterfallId
                })
                .Post("https://i.instagram.com/api/v1/accounts/validate_signup_sms_code/")
                .ToResponse<TraitResponse>();
        }

        public AccountCreateResponse Create(string day, string month, string year, bool registrationViaPhoneNumber = false, string verificationCode = "", bool isGdpr = false)
        {
            var timeNow = Utils.DateTimeNowTotalSeconds;

            string startNonce = registrationViaPhoneNumber
                ? $"{User.PhoneNumber}|{timeNow}|"
                : $"{User.Email.Username}|{timeNow}|";

            byte[] startNonceBytes = Encoding.ASCII.GetBytes(startNonce);

            byte[] nonceBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(startNonceBytes);
                    byte[] secureRandom = Utils.CreateSecureRandom();
                    bw.Write(secureRandom);
                }
                nonceBytes = ms.ToArray();
            }

            string snNonce = Utils.Base64Encode(nonceBytes);

            var registrationViaPhoneNumberParams = new
            {
                allow_contacts_sync = "true",
                verification_code = verificationCode,
                sn_result = "API_ERROR: null",
                phone_id = User.PhoneId,
                phone_number = User.PhoneNumber,
                _csrftoken = User.GetToken(),
                username = User.Username,
                first_name = User.FirstName,
                adid = User.AdvertisingId,
                guid = User.Uuid,
                device_id = User.DeviceId,
                sn_nonce = snNonce,
                force_sign_up_code = "",
                waterfall_id = User.RegisterWaterfallId,
                qs_stamp = "",
                password = User.Password,
                has_sms_consent = "true"
            };

            var registrationViaMailParams = new
            {
                is_secondary_account_creation = false,
                jazoest = CreateJazoest(User.PhoneId),
                tos_version = "row",
                suggestedUsername = "",
                allow_contacts_sync = "true",
                sn_result = "API_ERROR:+class+X.7Vg:null",
                phone_id = User.PhoneId,
                _csrftoken = User.GetToken(),
                username = User.Username,
                first_name = User.FirstName,
                day,
                adid = User.AdvertisingId,
                guid = User.Uuid,
                year,
                device_id = User.DeviceId,
                _uuid = User.Uuid,
                email = User.Email.Username,
                month,
                sn_nonce = snNonce,
                force_sign_up_code = "",
                waterfall_id = User.RegisterWaterfallId,
                qs_stamp = "",
                password = User.Password,
                one_tap_opt_in = "true"
            };

            var registrationViaMailWithGdprNumberParams = new
            {
                allow_contacts_sync = "true",
                sn_result = "API_ERROR: null",
                phone_id = User.PhoneId,
                gdpr_s = "[0,2,0,null]",
                _csrftoken = User.GetToken(),
                username = User.Username,
                first_name = User.FirstName,
                adid = User.AdvertisingId,
                guid = User.Uuid,
                device_id = User.DeviceId,
                email = User.Email.Username,
                sn_nonce = snNonce,
                force_sign_up_code = "",
                waterfall_id = User.RegisterWaterfallId,
                qs_stamp = "",
                password = User.Password,
            };

            var endpoint = registrationViaPhoneNumber
                ? "create_validated"
                : "create";

             User.Request
                .AddDefaultHeaders();

            if (registrationViaPhoneNumber)
                User.Request
                    .AddSignedParams(registrationViaPhoneNumberParams);
            else if (isGdpr)
                User.Request
                    .AddSignedParams(registrationViaMailWithGdprNumberParams);
            else
                User.Request
                    .AddSignedParams(registrationViaMailParams);

            return User.Request.Post($"https://i.instagram.com/api/v1/accounts/{endpoint}/")
            .ToResponse<AccountCreateResponse>();
        }

        public AccountCreateResponse CreateSecondaryAccount(string mainUserSessionToken, string mainUserId, string tosVersion, int day, int month, int year, string fullname, bool fillFullnameDirectly = false)
        {
            if (!fillFullnameDirectly) fullname = "";

            var timeNow = Utils.DateTimeNowTotalSeconds;

            string startNonce = $"{User.Username}|{timeNow}|";
            byte[] startNonceBytes = Encoding.ASCII.GetBytes(startNonce);

            byte[] nonceBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(startNonceBytes);
                    byte[] secureRandom = Utils.CreateSecureRandom();
                    bw.Write(secureRandom);
                }
                nonceBytes = ms.ToArray();
            }

            string snNonce = Utils.Base64Encode(nonceBytes);

            return User.Request
                .AddDefaultHeaders()
                .AddParam("main_user_session_token", mainUserSessionToken)
                .AddParam("tos_version", tosVersion)
                .AddParam("suggestedUsername", "")
                .AddParam("sn_result", "API_ERROR:+class+X.7Vg:null")
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("username", User.Username)
                .AddParam("first_name", "")
                .AddParam("day", day)
                .AddParam("adid", User.AdvertisingId)
                .AddParam("guid", User.Uuid)
                .AddParam("year", year)
                .AddParam("device_id", User.DeviceId)
                .AddParam("month", month)
                .AddParam("sn_nonce", snNonce)
                .AddParam("main_user_id", mainUserId)
                .AddParam("force_sign_up_code", "")
                .AddParam("waterfall_id", User.RegisterWaterfallId)
                .AddParam("password", "")
                .Post("https://i.instagram.com/api/v1/multiple_accounts/create_secondary_account/")
                .ToResponse<AccountCreateResponse>();
        }

        public UsernameSuggestionResponse CheckUsername(string username)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    username,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/users/check_username/")
                .ToResponse<UsernameSuggestionResponse>();
        }

        public UsernameSuggestionResponse UsernameSuggestion(string name)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_id = User.Uuid,
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    name,
                    device_id = User.DeviceId,
                    email = User.Email.Username,
                    waterfall_id = User.RegisterWaterfallId
                })
                .Post("https://i.instagram.com/api/v1/users/check_username/")
                .ToResponse<UsernameSuggestionResponse>();
        }

        public UserResponse ChangeProfilePicture(string uploadId, bool shareToFeed = false)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .AddParam("use_fbuploader", "true")
                .AddParam("share_to_feed", shareToFeed)
                .AddParam("upload_id", uploadId)
                .Post("https://i.instagram.com/api/v1/accounts/change_profile_picture/")
                .ToResponse<UserResponse>();
        }

        public TraitResponse SetReelSettings()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    reel_auto_archive = "on"
                })
                .Post("https://i.instagram.com/api/v1/users/set_reel_settings/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ProcessContactPointSignals()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    device_id = User.Uuid,
                    _uuid = User.Uuid,
                    google_tokens = "[]"

                })
                .Post("https://i.instagram.com/api/v1/accounts/process_contact_point_signals/")
                .ToResponse<TraitResponse>();
        }
    }
}
