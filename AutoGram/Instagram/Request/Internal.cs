using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using AutoGram.Helpers;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.CreateAccount;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Request
{
    class Internal : RequestCollection
    {
        public Internal(Instagram instagram) : base(instagram)
        {
        }

        public TraitResponse NewAccountNuxSeen()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    is_fb4a_installed = "false",
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid,
                    waterfall_id = User.RegisterWaterfallId
                })
                .Post("https://i.instagram.com/api/v1/nux/new_account_nux_seen/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse AcceptConsentRequired()
        {
            var response = User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    device_id = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/consent/new_user_flow_begins/")
                .ToResponse<TraitResponse>();

            Utils.RandomSleep(3000, 5000);

            response = User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    gdpr_s = "",
                    _csrftoken = User.GetToken(),
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    email = User.Email.Username
                })
                .Post("https://i.instagram.com/api/v1/consent/new_user_flow/")
                .ToResponse<TraitResponse>();

            Utils.RandomSleep(3000, 5000);


            response = User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    current_screen_key = "age_consent_two_button",
                    phone_id = User.PhoneId,
                    gdpr_s = "[0,0,0,null]",
                    _csrftoken = User.GetToken(),
                    updates = "{\"age_consent_state\":\"2\"}",
                    guid = User.Uuid,
                    device_id = User.DeviceId
                })
                .Post("https://i.instagram.com/api/v1/consent/new_user_flow/")
                .ToResponse<TraitResponse>();

            return response;
        }

        public void TermsAccept(string url)
        {
            var response = User.Request
                .SetCustomRequest()
                .AddHeader("Host", "b.i.instagram.com")
                .AddHeader("Connection", "keep-alive")
                .AddHeader("Cache-Control", "max-age=0")
                .AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
                .AddHeader("User-Agent", User.GetUserAgent())
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", $"{Constants.AcceptLanguage.Replace(" ", "")};q=0.8")
                .AddHeader("Cookie", "null")
                .AddHeader("X-Requested-With", "com.instagram.android")
                .Get(url).ToString();

            string rolloutHash = Utils.TryParse(response, "(?<=rollout_hash.:.)(.+?)(?=\")");

            Utils.RandomSleep(3000, 6000);

            User.Request
                .SetCustomRequest()
                .AddHeader("Host", "b.i.instagram.com")
                .AddHeader("Connection", "keep-alive")
                .AddHeader("Content-Length", "null")
                .AddHeader("Origin", "https://b.i.instagram.com")
                .AddHeader("X-Instagram-AJAX", rolloutHash)
                .AddHeader("User-Agent", User.GetUserAgent())
                .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                .AddHeader("Accept", "*/*")
                .AddHeader("X-Requested-With", "XMLHttpRequest")
                .AddHeader("X-CSRFToken", User.GetToken())
                .AddHeader("Referer", url)
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", $"{Constants.AcceptLanguage.Replace(" ", "")};q=0.8")
                .AddHeader("Cookie", "null")
                .Post("https://b.i.instagram.com/terms/accept/");
        }

        public TraitResponse AcquireOwnerContacts()
        {
            string pnSim = string.IsNullOrEmpty(User.PhoneNumber)
                ? Utils.GeneratePhoneNumber()
                : User.PhoneNumber;

            return User.Request
                .AddDefaultHeaders()
                .AddParam("phone_id", User.PhoneId)
                .AddParam("pn_sim", pnSim)
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("me", JsonConvert.SerializeObject(new
                {
                    phone_numbers = new int[0],
                    email_addresses = new int[0]
                }))
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/address_book/acquire_owner_contacts/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse AddressBookLink()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("contacts", JsonConvert.SerializeObject(RandomUserData.GetRandomContacts()))
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/address_book/link/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse DynamicOnboardingGetSteps(bool emptySeenSteps = false, bool isSecondaryAccountCreation = false, string progressState = "start", bool isCi = true)
        {
            var seenSteps = JsonConvert.SerializeObject(new[]
            {
                new
                {
                    step_name = "CHECK_FOR_PHONE",
                    value = 1
                },
                new
                {
                    step_name = "FB_CONNECT",
                    value = 0
                },
                new
                {
                    step_name = "FB_FOLLOW",
                    value = -1
                },
                new
                {
                    step_name = "FB_INVITE",
                    value = -1
                },
                new
                {
                    step_name = "CONTACT_INVITE",
                    value = -1
                },
                new
                {
                    step_name = "TAKE_PROFILE_PHOTO",
                    value = 1
                },
                new
                {
                    step_name = "ADD_PHONE",
                    value = -1
                },
                new
                {
                    step_name = "TURN_ON_ONETAP",
                    value = 1
                }
            });

            if (emptySeenSteps)
                seenSteps = "[]";

            return isSecondaryAccountCreation
                ? User.Request
                    .AddDefaultHeaders()
                    .AddSignedParams(new
                    {
                        is_secondary_account_creation = isSecondaryAccountCreation,
                        fb_connected = false,
                        seen_steps = seenSteps,
                        progress_state = progressState,
                        phone_id = User.PhoneId,
                        fb_installed = "false",
                        locale = User.Device.GetUserAgentLocale,
                        timezone_offset = User.TimezoneOffset,
                        _csrftoken = User.GetToken(),
                        network_type = "WIFI-UNKNOWN",
                        _uid = User.AccountId,
                        guid = User.Uuid,
                        _uuid = User.Uuid,
                        is_ci = "false",
                        android_id = User.DeviceId,
                        waterfall_id = User.RegisterWaterfallId,
                        tos_accepted = "true"
                    })
                    .Post("https://b.i.instagram.com/api/v1/dynamic_onboarding/get_steps/")
                    .ToResponse<TraitResponse>()
                : User.Request
                    .AddDefaultHeaders()
                    .AddSignedParams(new
                    {
                        is_secondary_account_creation = isSecondaryAccountCreation,
                        fb_connected = false,
                        seen_steps = seenSteps,
                        progress_state = progressState,
                        phone_id = User.PhoneId,
                        fb_installed = "false",
                        locale = User.Device.GetUserAgentLocale,
                        timezone_offset = User.TimezoneOffset,
                        _csrftoken = User.GetToken(),
                        network_type = "WIFI-UNKNOWN",
                        _uid = User.AccountId,
                        guid = User.Uuid,
                        _uuid = User.Uuid,
                        is_ci = "false",
                        android_id = User.DeviceId,
                        waterfall_id = User.RegisterWaterfallId,
                        reg_flow_taken = "email",
                        tos_accepted = "true"
                    })
                    .Post("https://b.i.instagram.com/api/v1/dynamic_onboarding/get_steps/")
                    .ToResponse<TraitResponse>();
        }

        public TraitResponse GetInviteSuggestions(bool isFirst = true)
        {
            User.Request
                .AddDefaultHeaders();

            if (isFirst)
            {
                User.Request
                    .AddParam("count_only", "1")
                    .AddParam("_csrftoken", User.GetToken())
                    .AddParam("_uuid", User.Uuid);
            }
            else
            {
                User.Request
                    .AddParam("offset", 0)
                    .AddParam("_csrftoken", User.GetToken())
                    .AddParam("_uuid", User.Uuid)
                    .AddParam("count", 50);
            }

            return User.Request
                 .Post("https://i.instagram.com/api/v1/fb/get_invite_suggestions/")
                 .ToResponse<TraitResponse>();
        }

        public TraitResponse ReelsTray(string reason = "pull_to_refresh")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("supported_capabilities_new", Constants.SupportedCapabilities)
                .AddParam("reason", reason)
                .AddParam("timezone_offset", User.TimezoneOffset)
                .AddParam("tray_session_id", Utils.GenerateUUID(true))
                .AddParam("request_id", Utils.GenerateUUID(true))
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/feed/reels_tray/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse LogAcquirablePhoneNumber()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    can_prefill_from_sim = "true",
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    device_id = User.Uuid,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/log_acquirable_phone_number/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse RankedRecipients(string mode)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("mode", mode)
                .AddUrlParam("show_threads", "true")
                .AddUrlParam("use_unified_inbox", "true")
                .Get("https://i.instagram.com/api/v1/direct_v2/ranked_recipients/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse RankedRecipients(string mode, string showThreads, string query)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("mode", mode)
                .AddUrlParam("show_threads", showThreads)
                .AddUrlParam("query", query)
                .Get("https://i.instagram.com/api/v1/direct_v2/ranked_recipients/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse DirectInbox(string prefetchRequest = null, string limit = "0", string threadMessageLimit = null, string fetchReason = null)
        {
            User.Request
                .AddDefaultHeaders(prefetchRequest: prefetchRequest)
                .AddUrlParam("visual_message_return_type", "unseen")
                .AddUrlParam("persistentBadging", "true")
                .AddUrlParam("limit", limit);

            if (!string.IsNullOrEmpty(fetchReason))
                User.Request.AddUrlParam("fetch_reason", fetchReason);

            if (!string.IsNullOrEmpty(threadMessageLimit))
                User.Request.AddUrlParam("thread_message_limit", threadMessageLimit);

            return User.Request
             .Get("https://i.instagram.com/api/v1/direct_v2/inbox/")
             .ToResponse<TraitResponse>();
        }

        public TraitResponse NewsInbox(string prefetchRequest = null)
        {
            return User.Request
                .AddDefaultHeaders(prefetchRequest: prefetchRequest)
                .Get("https://i.instagram.com/api/v1/news/inbox/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse DirectGetPresence()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/direct_v2/get_presence/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse QpFetch(string surfaceParam = "4715")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    vc_policy = "default",
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    query = Constants.FetchQuery,
                    scale = "1",
                    version = "1",
                    surface_param = surfaceParam
                })
                .Post("https://i.instagram.com/api/v1/qp/fetch/")
                .ToResponse<TraitResponse>();
        }

        public void FetchHeaders()
        {
            User.Request
                .AddDefaultHeaders()
                .Get(
                    $"https://i.instagram.com/api/v1/si/fetch_headers/?guid={User.Uuid.Replace("-", "")}&challenge_type=signup");
        }

        public TraitResponse ProfileNotice()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/users/profile_notice/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse MediaBlocked()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/media/blocked/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse DiscoverExplore(string prefetchRequest = null)
        {
            return User.Request
                .AddDefaultHeaders(prefetchRequest: prefetchRequest)
                .AddUrlParam("is_prefetch", "true")
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .AddUrlParam("session_id", User.SessionId)
                .Get("https://i.instagram.com/api/v1/discover/explore/")
                .ToResponse<TraitResponse>();
        }

        public void FetchOneTap()
        {
            User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    guid = User.Uuid,
                    device_id = User.DeviceId,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/fetch_onetap/");
        }

        public TraitResponse DiscoverProfileSuBadge()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/discover/profile_su_badge/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse HighLightsTray(string userId = null)
        {
            if (userId == null) userId = User.AccountId;

            return User.Request
                .AddCustomHeader("X-Ads-Opt-Out", "0")
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddCustomHeader("phone_id", User.PhoneId)
                .AddCustomHeader("battery_level", User.BatteryLevel.ToString())
                .AddCustomHeader("is_charging", User.IsCharging.ToString())
                .AddCustomHeader("is_dark_mode", "0")
                .AddCustomHeader("will_sound_on", "0")
                .AddDefaultHeaders()
                .AddUrlParam("supported_capabilities_new", "[{\"name\":\"SUPPORTED_SDK_VERSIONS\",\"value\":\"103.0,104.0,105.0,106.0,107.0,108.0,109.0,110.0,111.0,112.0,113.0,114.0,115.0,116.0,117.0,118.0,119.0,120.0,121.0\"},{\"name\":\"FACE_TRACKER_VERSION\",\"value\":\"14\"},{\"name\":\"segmentation\",\"value\":\"segmentation_enabled\"},{\"name\":\"COMPRESSION\",\"value\":\"ETC2_COMPRESSION\"},{\"name\":\"world_tracker\",\"value\":\"world_tracker_enabled\"},{\"name\":\"gyroscope\",\"value\":\"gyroscope_enabled\"}]")
                .Get($"https://i.instagram.com/api/v1/highlights/{userId}/highlights_tray/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse InjectedReelsMedia()
        {
            return User.Request
                .AddCustomHeader("X-Ads-Opt-Out", "0")
                .AddCustomHeader("X-Google-AD-ID", User.AdvertisingId)
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddCustomHeader("battery_level", User.BatteryLevel.ToString())
                .AddCustomHeader("is_charging", User.IsCharging.ToString())
                .AddCustomHeader("is_dark_mode", "0")
                .AddCustomHeader("will_sound_on", "0")
                .AddCustomHeader("phone_id", User.PhoneId)
                .AddCustomHeader("X-CM-Bandwidth-KBPS", "-1.000")
                .AddCustomHeader("X-CM-Latency", "-1.000")
                .AddDefaultHeaders()
                .Post($"https://i.instagram.com/api/v1/feed/injected_reels_media/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse AccountLinking()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("signed_body", "SIGNATURE.{}")
                .Get("https://i.instagram.com/api/v1/ig_fb_xposting/account_linking/user_xposting_destination/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetPresenceDisabled()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("signed_body", Utils.GetSHA256("", User.App.SignatureKey, true))
                .AddUrlParam("ig_sig_key_version", Constants.SigVersion)
                .Get("https://i.instagram.com/api/v1/accounts/get_presence_disabled/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ScoresBootstrap()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParamDecode("surfaces",
                    "[\"autocomplete_user_list\",\"coefficient_besties_list_ranking\",\"coefficient_rank_recipient_user_suggestion\",\"coefficient_ios_section_test_bootstrap_ranking\",\"coefficient_direct_recipients_ranking_variant_2\"]")
                .Get("https://i.instagram.com/api/v1/scores/bootstrap/users/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FacebookOta()
        {
            return User.Request
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddUrlParam("fields", Constants.FacebookOtaFields)
                .AddUrlParam("custom_user_id", User.AccountId)
                .AddUrlParam("signed_body", Utils.GetSHA256("", User.App.SignatureKey, true))
                .AddUrlParam("ig_sig_key_version", Constants.SigVersion)
                .AddUrlParam("version_code", User.App.Code)
                .AddUrlParam("version_name", User.App.Name)
                .AddUrlParam("custom_app_id", Constants.FacebookAppId)
                .AddUrlParam("custom_device_id", User.Uuid)
                .Get("https://i.instagram.com/api/v1/facebook_ota/")
                .ToResponse<TraitResponse>();
        }

        public MediaConfigureResponse UploadAlbum(List<MediaObject> mediaObjects)
        {
            if (mediaObjects.Count < 2)
            {
                throw new ArgumentException();
            }

            string uploadId = Utils.GenerateUploadId();

            string uploadIdFirst = UploadAlbumPhoto(mediaObjects[0]);
            string uploadIdSecond = UploadAlbumPhoto(mediaObjects[1]);
            //string uploadIdThird = UploadAlbumPhoto(mediaObjects[2]);

            Utils.RandomSleep(12000, 32000);

            return User.Request
                .AddCustomHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3000)}kbps")
                .AddCustomHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000")
                .AddCustomHeader("X-IG-Bandwidth-TotalBytes-B", "0")
                .AddCustomHeader("X-IG-Bandwidth-TotalTime-MS", "0")
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddSignedParams(new
                {
                    timezone_offset = User.TimezoneOffset,
                    _csrftoken = User.GetToken(),
                    source_type = "4",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    caption = mediaObjects[0].Caption != "" ? Utils.EncodeNonAsciiCharacters(mediaObjects[0].Caption) : "",
                    client_sidecar_id = uploadId,
                    upload_id = uploadId,
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    children_metadata = new[]
                    {
                        new
                        {
                            timezone_offset = User.TimezoneOffset,
                            source_type = "4",
                            upload_id = uploadIdFirst,
                            caption = "null",
                            device = new
                            {
                                manufacturer = User.Device.GetManufacturer,
                                model = User.Device.GetModel,
                                android_version = int.Parse(User.Device.GetAndroidVersion),
                                android_release = User.Device.GetAndroidRelease
                            },
                            extra = new
                            {
                                source_width = mediaObjects[0].Width,
                                source_height = mediaObjects[0].Height
                            },
                            edits = new
                            {
                                crop_original_size = new[]
                                {
                                    (float)mediaObjects[0].Width,
                                    (float)mediaObjects[0].Height
                                },
                                crop_center = new[]
                                {
                                    0.0,
                                    -0.0
                                },
                                crop_zoom = 1.0
                            }
                        },
                        new
                        {
                            timezone_offset = User.TimezoneOffset,
                            source_type = "4",
                            upload_id = uploadIdSecond,
                            caption = "null",
                            device = new
                            {
                                manufacturer = User.Device.GetManufacturer,
                                model = User.Device.GetModel,
                                android_version = int.Parse(User.Device.GetAndroidVersion),
                                android_release = User.Device.GetAndroidRelease
                            },
                            extra = new
                            {
                                source_width = mediaObjects[1].Width,
                                source_height = mediaObjects[1].Height
                            },
                            edits = new
                            {
                                crop_original_size = new[]
                                {
                                    (float)mediaObjects[1].Width,
                                    (float)mediaObjects[1].Height
                                },
                                crop_center = new[]
                                {
                                    0.0,
                                    -0.0
                                },
                                crop_zoom = 1.0
                            }
                        }
                    }
                }, true)
                .Post("https://i.instagram.com/api/v1/media/configure_sidecar/")
                .ToResponse<MediaConfigureResponse>();
        }

        public string UploadAlbumPhoto(MediaObject mediaObject)
        {
            string boundary = Utils.GenerateBoundary();

            string uploadId = Utils.GenerateUploadId();
            string photoId = (uploadId + ".jpg").GetHashCode().ToString();
            string photoName = $"pending_media_{photoId}.jpg";

            var sb = new StringBuilder();
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"is_sidecar\"");
            sb.Append("\r\n\r\n");
            sb.Append("1");
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"_csrftoken\"");
            sb.Append("\r\n\r\n");
            sb.Append(User.GetToken());
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"_uuid\"");
            sb.Append("\r\n\r\n");
            sb.Append(User.Uuid);
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append($"Content-Disposition: form-data; name=\"photo\"; filename=\"{photoName}\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: application/octet-stream");
            sb.Append("\r\n");
            sb.Append("Content-Transfer-Encoding: binary");
            sb.Append("\r\n\r\n");

            byte[] a = Encoding.UTF8.GetBytes(sb.ToString());
            sb.Clear();

            byte[] b = mediaObject.Image;

            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"upload_id\"");
            sb.Append("\r\n\r\n");
            sb.Append(uploadId);
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"image_compression\"");
            sb.Append("\r\n\r\n");
            sb.Append("{\"lib_name\":\"moz\",\"lib_version\":\"3.1.m\",\"quality\":\"89\"}");
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"retry_context\"");
            sb.Append("\r\n\r\n");
            sb.Append("{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}");
            sb.Append("\r\n");

            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"media_type\"");
            sb.Append("\r\n\r\n");
            sb.Append("1");
            sb.Append("\r\n");
            sb.Append("--" + boundary + "--\r\n");
            byte[] c = Encoding.UTF8.GetBytes(sb.ToString());

            byte[] r = a.Concat(b).Concat(c).ToArray();

            User.Request
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/upload/photo/", r,
                    "multipart/form-data; boundary=" + boundary);

            return uploadId;
        }

        public MediaConfigureResponse UploadPhoto(MediaObject mediaObject, bool isDirect = false, string threadId = null, bool isStory = false)
        {
            string watterFallId = Utils.GenerateUUID(true);
            string uploadId = Utils.GenerateUploadId();

            string photoId = (uploadId + ".jpg").GetHashCode().ToString();
            string photoName = $"{uploadId}_0_{photoId}";

            string uploadPhotoParams =
                $"{{\"retry_context\":\"{{\\\"num_step_auto_retry\\\":0,\\\"num_reupload\\\":0,\\\"num_step_manual_retry\\\":0}}\"," +
                $"\"image_compression\":\"{{\\\"lib_name\\\":\\\"moz\\\",\\\"lib_version\\\":\\\"3.1.m\\\",\\\"quality\\\":\\\"87\\\"}}\"," +
                $"\"media_type\":\"1\",\"upload_id\":\"{uploadId}\"}}";

            User.Request
                .AddDefaultHeaders()
                .AddHeader("X_FB_PHOTO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Instagram-Rupload-Params", uploadPhotoParams)
                //.AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                //.AddHeader("X-IG-Capabilities", User.App.Capabilities)
                //.AddHeader("X-IG-App-ID", Constants.InstagramAppId)
                .Get($"https://i.instagram.com/rupload_igphoto/{photoName}");

            Utils.RandomSleep(1000, 2600);

            User.Request
                .AddDefaultHeaders()
                .AddHeader("X_FB_PHOTO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Entity-Type", "image/jpeg")
                .AddHeader("Offset", "0")
                .AddHeader("X-Instagram-Rupload-Params", uploadPhotoParams)
                .AddHeader("X-Entity-Name", photoName)
                .AddHeader("X-Entity-Length", mediaObject.Image.Length.ToString())
                //.AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                //.AddHeader("X-IG-Capabilities", User.App.Capabilities)
                //.AddHeader("X-IG-App-ID", Constants.InstagramAppId)
                .Post($"https://i.instagram.com/rupload_igphoto/{photoName}", mediaObject.Image);

            Utils.RandomSleep(3000, 6000);

            if (isDirect)
                return MediaConfigureDirectPhoto(mediaObject, uploadId, threadId);
            else if (isStory)
                return MediaConfigureStory(mediaObject, uploadId);
            else
                return MediaConfigurePhoto(mediaObject, uploadId);
        }

        public MediaConfigureResponse UploadPhotoSkipConfiguration(MediaObject mediaObject)
        {
            string watterFallId = Utils.GenerateUUID(true);
            string uploadId = Utils.GenerateUploadId();

            string photoId = (uploadId + ".jpg").GetHashCode().ToString();
            string photoName = $"{uploadId}_0_{photoId}";

            string uploadPhotoParams =
                $"{{\"retry_context\":\"{{\\\"num_step_auto_retry\\\":0,\\\"num_reupload\\\":0,\\\"num_step_manual_retry\\\":0}}\"," +
                $"\"image_compression\":\"{{\\\"lib_name\\\":\\\"moz\\\",\\\"lib_version\\\":\\\"3.1.m\\\",\\\"quality\\\":\\\"87\\\"}}\"," +
                $"\"media_type\":\"1\",\"upload_id\":\"{uploadId}\"}}";

            User.Request
                .AddDefaultHeaders()
                .AddHeader("X_FB_PHOTO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Instagram-Rupload-Params", uploadPhotoParams)
                .Get($"https://i.instagram.com/rupload_igphoto/{photoName}");

            Utils.RandomSleep(1000, 2600);

            return User.Request
                .AddDefaultHeaders()
                .AddHeader("X_FB_PHOTO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Entity-Type", "image/jpeg")
                .AddHeader("Offset", "0")
                .AddHeader("X-Instagram-Rupload-Params", uploadPhotoParams)
                .AddHeader("X-Entity-Name", photoName)
                .AddHeader("X-Entity-Length", mediaObject.Image.Length.ToString())
                .Post($"https://i.instagram.com/rupload_igphoto/{photoName}", mediaObject.Image)
                .ToResponse<MediaConfigureResponse>();
        }

        public MediaConfigureResponse MediaConfigureStory(MediaObject mediaObject, string uploadId)
        {
            return User.Request
                .AddCustomHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3000)}kbps")
                .AddCustomHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000")
                .AddCustomHeader("X-IG-Bandwidth-TotalBytes-B", "0")
                .AddCustomHeader("X-IG-Bandwidth-TotalTime-MS", "0")
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddSignedParams(new
                {
                    timezone_offset = User.TimezoneOffset,
                    _csrftoken = User.GetToken(),
                    client_shared_at = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(25, 55)).ToString(),
                    media_folder = "Instagram",
                    configure_mode = "1",
                    source_type = "4",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    imported_taken_at = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(500, 1000)).ToString(),
                    capture_type = "normal",
                    mas_opt_in = "NOT_PROMPTED",
                    upload_id = uploadId,
                    client_timestamp = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(20, 25)).ToString(),
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    edits = new
                    {
                        crop_original_size = new[]
                        {
                            (float)mediaObject.Width,
                            (float)mediaObject.Height
                        },
                        crop_center = new[]
                        {
                            0.0,
                            -0.0
                        },
                        crop_zoom = 1.0
                    },
                    extra = new
                    {
                        source_width = mediaObject.Width,
                        source_height = mediaObject.Height
                    }
                }, true)
                .Post("https://i.instagram.com/api/v1/media/configure_to_story/")
                .ToResponse<MediaConfigureResponse>();
        }

        public MediaConfigureResponse MediaConfigureVideoStory(MediaObject mediaObject, string uploadId)
        {
            return User.Request
                .AddCustomHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3000)}kbps")
                .AddCustomHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000")
                .AddCustomHeader("X-IG-Bandwidth-TotalBytes-B", "0")
                .AddCustomHeader("X-IG-Bandwidth-TotalTime-MS", "0")
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddSignedParams(new
                {
                    allow_multi_configures = "1",
                    filter_type = "0",
                    timezone_offset = User.TimezoneOffset,
                    _csrftoken = User.GetToken(),
                    client_shared_at = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(25, 55)).ToString(),
                    media_folder = "Camera",
                    configure_mode = "1",
                    source_type = "4",
                    video_result = "",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    imported_taken_at = (DateTime.UtcNow.ToUnixTime()).ToString(),
                    date_time_original = DateTime.Now.ToString("yyyyMMddTHHmmss") + ".000Z",
                    capture_type = "normal",
                    audience = "default",
                    mas_opt_in = "NOT_PROMPTED",
                    upload_id = uploadId,
                    client_timestamp = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(20, 25)).ToString(),
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    length = 4.412,
                    clips = new[]
                    {
                        new
                        {
                            length = 4.412,
                            source_type = "4"
                        }
                    },
                    extra = new
                    {
                        source_width = 640,
                        source_height = 1138
                    },
                    audio_muted = false,
                    poster_frame_index = 0
                }, true)
                .Post("https://i.instagram.com/api/v1/media/configure_to_story/?video=1")
                .ToResponse<MediaConfigureResponse>();
        }

        public MediaConfigureResponse MediaConfigurePhoto(MediaObject mediaObject, string uploadId)
        {

            return User.Request
                .AddDefaultHeaders()
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddSignedParams(new
                {
                    timezone_offset = User.TimezoneOffset,
                    _csrftoken = User.GetToken(),
                    media_folder = "Instagram",
                    source_type = "4",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    caption = mediaObject.Caption,
                    upload_id = uploadId,
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    edits = new
                    {
                        crop_original_size = new[]
                        {
                            (float)mediaObject.Width,
                            (float)mediaObject.Height
                        },
                        crop_center = new[]
                        {
                            0.0,
                            -0.0
                        },
                        crop_zoom = 1.0
                    },
                    extra = new
                    {
                        source_width = mediaObject.Width,
                        source_height = mediaObject.Height
                    }
                }, true)
                .Post("https://i.instagram.com/api/v1/media/configure/")
                .ToResponse<MediaConfigureResponse>();
        }

        public MediaConfigureResponse MediaConfigureDirectPhoto(MediaObject mediaObject, string uploadId, string threadId)
        {
            return User.Request
                .AddCustomHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3000)}kbps")
                .AddCustomHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000")
                .AddCustomHeader("X-IG-Bandwidth-TotalBytes-B", "0")
                .AddCustomHeader("X-IG-Bandwidth-TotalTime-MS", "0")
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    thread_ids = $"[{threadId}]",
                    client_shared_at = (DateTime.UtcNow.ToUnixTime() - Utils.Random.Next(25, 55)).ToString(),
                    source_type = "3",
                    configure_mode = "2",
                    client_timestamp = DateTime.UtcNow.ToUnixTime(),
                    upload_id = uploadId,
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    edits = new
                    {
                        crop_original_size = new[]
                        {
                            (float)mediaObject.Width,
                            (float)mediaObject.Height
                        },
                        crop_center = new[]
                        {
                            0.0,
                            -0.0
                        },
                        crop_zoom = 1.0
                    },
                    extra = new
                    {
                        source_width = mediaObject.Width,
                        source_height = mediaObject.Height
                    }
                }, true)
                .Post("https://i.instagram.com/api/v1/media/configure_to_story/")
                .ToResponse<MediaConfigureResponse>();
        }

        public MediaConfigureResponse UploadVideo(MediaObject mediaObject, bool isStory = false)
        {
            // Upload video params

            string watterFallId = Utils.GenerateUUID(true);
            string uploadId = Utils.GenerateUploadId();

            string videoId = (uploadId + ".mp4").GetHashCode().ToString();
            string videoName = $"{uploadId}_0_{videoId}";

            string photoId = (uploadId + ".jpg").GetHashCode().ToString();
            string photoName = $"{uploadId}_0_{photoId}";

            // todo: Media.Duration
            string videoDurationInMsec = "4412";

            string uploadVideoParams =
                $"{{\"retry_context\":\"{{\\\"num_step_auto_retry\\\":0,\\\"num_reupload\\\":0,\\\"num_step_manual_retry\\\":0}}\"," +
                $"\"upload_media_width\":\"{mediaObject.Width}\",\"media_type\":\"2\",\"upload_id\":\"{uploadId}\"," +
                $"\"upload_media_duration_ms\":\"{videoDurationInMsec}\",\"upload_media_height\":\"{mediaObject.Height}\"}}";

            if (isStory)
            {
                uploadVideoParams = "{\"upload_media_height\":\"1138\",\"xsharing_user_ids\":\"[]\",\"upload_media_width\":\"640\"," +
                    $"\"for_direct_story\":\"1\",\"upload_media_duration_ms\":\"4412\",\"upload_id\":\"{uploadId}\",\"for_album\":" +
                    "\"1\",\"retry_context\":\"{\\\"num_step_auto_retry\\\":0,\\\"num_reupload\\\":0,\\\"num_step_manual_retry\\\":0}\"," +
                    "\"media_type\":\"2\",\"potential_share_types\":[\"story\",\"direct_story\"]}";
            }

            string uploadPhotoParams =
                $"{{\"upload_id\":\"{uploadId}\",\"image_compression\":\"{{\\\"lib_name\\\":\\\"moz\\\",\\\"lib_version\\\":\\\"3.1.m\\\"," +
                $"\\\"quality\\\":\\\"85\\\"}}\",\"retry_context\":\"{{\\\"num_step_auto_retry\\\":0,\\\"num_reupload\\\":0," +
                $"\\\"num_step_manual_retry\\\":0}}\",\"media_type\":\"2\"}}";


            // Pre-Upload video request

            User.Request
                .AddHeader("X_FB_VIDEO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Instagram-Rupload-Params", uploadVideoParams)
                .AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddHeader("X-IG-App-ID", Constants.InstagramAppId)
                .Get($"https://i.instagram.com/rupload_igvideo/{videoName}");

            // Upload video request

            User.Request
                .AddHeader("X-Entity-Name", videoName)
                .AddHeader("X-Entity-Length", mediaObject.Video.Length.ToString())
                .AddHeader("X_FB_VIDEO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Entity-Type", "video/webm")
                .AddHeader("X-Instagram-Rupload-Params", uploadVideoParams)
                .AddHeader("Offset", "0")
                .AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddHeader("X-IG-App-ID", Constants.InstagramAppId)
                .Post($"https://i.instagram.com/rupload_igvideo/{videoName}", mediaObject.Video);

            // Upload photo request

            User.Request
                .AddHeader("X_FB_PHOTO_WATERFALL_ID", watterFallId)
                .AddHeader("X-Entity-Name", photoName)
                .AddHeader("X-Entity-Length", mediaObject.Image.Length.ToString())
                .AddHeader("X-Entity-Type", "image/jpeg")
                .AddHeader("X-Instagram-Rupload-Params", uploadPhotoParams)
                .AddHeader("Offset", "0")
                .AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddHeader("X-IG-App-ID", Constants.InstagramAppId)
                .Post($"https://i.instagram.com/rupload_igphoto/{photoName}", mediaObject.Image);

            return isStory
                ? MediaConfigureVideoStory(mediaObject, uploadId)
                : MediaConfigureVideo(mediaObject, uploadId);
        }

        public MediaConfigureResponse MediaConfigureVideo(MediaObject mediaObject, string uploadId)
        {
            double videoLenght = 3.05;

            return User.Request
                .AddCustomHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3000)}kbps")
                .AddCustomHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000")
                .AddCustomHeader("X-IG-Bandwidth-TotalBytes-B", "0")
                .AddCustomHeader("X-IG-Bandwidth-TotalTime-MS", "0")
                .AddCustomHeader("retry_context", "{\"num_step_auto_retry\":0,\"num_reupload\":0,\"num_step_manual_retry\":0}")
                .AddCustomHeader("X-IG-Connection-Type", Constants.InstagramConnectionType)
                .AddCustomHeader("X-IG-Capabilities", User.App.Capabilities)
                .AddCustomHeader("X-IG-App-ID", Constants.InstagramAppId)
                .AddSignedParams(new
                {
                    filter_type = "0",
                    timezone_offset = "7200",
                    _csrftoken = User.GetToken(),
                    source_type = "4",
                    video_result = "",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    caption = mediaObject.Caption != "" ? Utils.EncodeNonAsciiCharacters(mediaObject.Caption) : "",
                    date_time_original = DateTime.Now.ToString("yyyyMMddTHHmmss") + ".000Z",
                    upload_id = uploadId,
                    device = new
                    {
                        manufacturer = User.Device.GetManufacturer,
                        model = User.Device.GetModel,
                        android_version = int.Parse(User.Device.GetAndroidVersion),
                        android_release = User.Device.GetAndroidRelease
                    },
                    length = videoLenght,
                    clips = new[]
                    {
                        new
                        {
                            length = videoLenght,
                            source_type = "4"
                        }
                    },
                    extra = new
                    {
                        source_width = mediaObject.Width,
                        source_height = mediaObject.Height
                    },
                    audio_muted = false,
                    poster_frame_index = 0
                }, true)
                .AddUrlParam("video", "1")
                .Post("https://i.instagram.com/api/v1/media/configure/")
                .ToResponse<MediaConfigureResponse>();
        }

        public void CheckFbFriends()
        {
            User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/fb/fb_entrypoint_info/");
        }

        public TraitResponse FetchZeroRatingToken(string reason = "token_expired")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("device_id", User.DeviceId)
                .AddUrlParam("token_hash", "")
                .AddUrlParam("custom_device_id", User.Uuid)
                .AddUrlParam("fetch_reason", reason)
                .Get("https://i.instagram.com/api/v1/zr/token/result/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse BootstrapMsisdnHeader(string usage = "ig_select_app")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    mobile_subno_usage = usage,
                    device_id = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/msisdn_header_bootstrap/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ReadMsisdnHeader(string usage = "default", bool useCsrfToken = false, bool isLoggedUsage = false)
        {
            if (useCsrfToken)
            {
                User.Request.AddSignedParams(new
                {
                    mobile_subno_usage = usage,
                    device_id = User.Uuid,
                    _csrftoken = User.GetToken()
                });
            }
            else if (isLoggedUsage)
            {
                User.Request.AddSignedParams(new
                {
                    mobile_subno_usage = usage,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    device_id = User.Uuid,
                    _uuid = User.Uuid
                });
            }
            else
            {
                User.Request.AddSignedParams(new
                {
                    mobile_subno_usage = usage,
                    device_id = User.Uuid
                });
            }

            return User.Request
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/accounts/read_msisdn_header/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SetContactPointPrefill(string usage = "prefill", bool useCsrfToken = false, bool isLoggedUsage = false, bool isCreatedAccountUsage = false)
        {
            if (isCreatedAccountUsage)
            {
                User.Request.AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    device_id = User.Uuid,
                    _uuid = User.Uuid,
                    usage
                });
            }
            if (isLoggedUsage)
            {
                User.Request.AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    usage
                });
            }
            else if (useCsrfToken)
            {
                User.Request.AddSignedParams(new
                {
                    phone_id = User.PhoneId,
                    _csrftoken = User.GetToken(),
                    usage
                });
            }
            else
            {
                User.Request.AddSignedParams(new
                {
                    phone_id = User.Uuid,
                    usage
                });
            }

            return User.Request
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/accounts/contact_point_prefill/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SyncDeviceFeatures(bool preLogin = false, bool useCsrfToken = false)
        {
            if (preLogin && !useCsrfToken)
            {
                User.Request
                    .AddSignedParams(new
                    {
                        id = User.Uuid,
                        experiments = Constants.LoginExperiments
                    });
            }
            else if (preLogin)
            {
                User.Request
                    .AddSignedParams(new
                    {
                        id = User.Uuid,
                        _csrftoken = User.GetToken(),
                        experiments = Constants.LoginExperiments
                    });
            }
            else
            {
                User.Request
                    .AddSignedParams(new
                    {
                        id = User.Uuid,
                        experiments = Constants.LoginExperiments,
                        _uuid = User.Uuid,
                        _uid = User.AccountId
                    });
            }

            return User.Request
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/qe/sync/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SyncUserFeatures()
        {
            User.Request
                .AddSignedParams(new
                {
                    _uuid = User.Uuid,
                    _uid = User.AccountId,
                    id = User.AccountId,
                    experiments = Constants.Experiments
                });

            return User.Request
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/qe/sync/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse PreLoginLauncherSync(bool useCsrfToken = false)
        {
            if (useCsrfToken)
            {
                User.Request
                    .AddSignedParams(new
                    {
                        id = User.Uuid,
                        server_config_retrieval = "1",
                        _csrftoken = User.GetToken()
                    });
            }
            else
            {
                User.Request
                    .AddSignedParams(new
                    {
                        id = User.Uuid,
                        server_config_retrieval = "1"
                    });
            }


            return User.Request
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/launcher/sync/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse PostLoginLauncherSync(bool idIsUuid = false)
        {
            string id = idIsUuid ? User.Uuid : User.AccountId;

            User.Request
                .AddSignedParams(new
                {
                    id,
                    server_config_retrieval = "1",
                    _uuid = User.Uuid,
                    _uid = User.AccountId
                });

            return User.Request
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/launcher/sync/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse LogAttribution()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    adid = User.AdvertisingId
                })
                .Post("https://i.instagram.com/api/v1/attribution/log_attribution/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetPrefillCandidates()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    android_device_id = User.DeviceId,
                    usages = "[\"account_recovery_omnibox\"]",
                    device_id = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/accounts/get_prefill_candidates/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetSharePrefill(string views)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParamDecode("views", views)
                .Get("https://b.i.instagram.com/api/v1/banyan/banyan/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse LoomFetchConfig()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/loom/fetch_config/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse HasInteropUpgraded()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/direct_v2/has_interop_upgraded/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse IgtvBrowseFeed()
        {
            return User.Request
                .AddDefaultHeaders(prefetchRequest: "foreground")
                .AddUrlParam("prefetch", "1")
                .Get("https://i.instagram.com/api/v1/igtv/browse_feed/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse IgtvChannel(string userPk)
        {
            return User.Request
                .AddCustomHeader("X-Ads-Opt-Out", "0")
                .AddCustomHeader("X-Google-AD-ID", User.AdvertisingId)
                .AddCustomHeader("X-DEVICE-ID", User.Uuid)
                .AddCustomHeader("X-CM-Bandwidth-KBPS", "-1.000")
                .AddCustomHeader("X-CM-Latency", "-1.000")
                .AddDefaultHeaders()
                .AddUrlParam("phone_id", User.PhoneId)
                .AddUrlParam("battery_level", User.BatteryLevel)
                .AddUrlParam("_csrftoken", User.GetToken())
                .AddUrlParam("id", $"user_{userPk}")
                .AddUrlParam("_uuid", User.Uuid)
                .AddUrlParam("is_charging", User.IsCharging)
                .AddUrlParam("will_sound_on", "0")
                .Get("https://i.instagram.com/api/v1/igtv/browse_feed/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ReelsMedia(string userId, string source = "feed_timeline")
        {
            var userIds = new string[] { userId };

            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    supported_capabilities_new = Constants.SupportedCapabilities,
                    source,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    user_ids = userIds
                })
                .Post("https://i.instagram.com/api/v1/feed/reels_media/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse LogResurrectAttribution()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    adid = User.AdvertisingId,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/attribution/log_resurrect_attribution/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FetchConfig()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/loom/fetch_config/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetCooldowns()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("signed_body", "SIGNATURE.{}")
                .Get("https://i.instagram.com/api/v1/qp/get_cooldowns/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ArlinkDownloadInfo()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("version_override", "2.2.1")
                .Get("https://i.instagram.com/api/v1/users/arlink_download_info/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetLinkageStatus()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/linked_accounts/get_linkage_status/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse WriteSupportedCapabilities()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    supported_capabilities_new = Constants.SupportedCapabilities,
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/creatives/write_supported_capabilities/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetViewableStatuses()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("include_authors", "true")
                .Get("https://i.instagram.com/api/v1/status/get_viewable_statuses/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GraphQl(string friendlyName, string docId, string queryParams)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddCustomHeader("X-FB-Friendly-Name", friendlyName)
                .AddParam("signed_body", "SIGNATURE.")
                .AddParam("doc_id", docId)
                .AddParam("locale", User.Device.GetUserAgentLocale)
                .AddParam("vc_policy", "default")
                .AddParam("strip_nulls", "true")
                .AddParam("query_params", queryParams)
                .Post("https://i.instagram.com/api/v1/wwwgraphql/ig/query/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse QpBatchFetch()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    vc_policy = "default",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    surfaces_to_triggers = Constants.SurfacesToTriggers,
                    surfaces_to_queries = Constants.SurfacesToQueries.Replace("(", "%028").Replace(")", "%029"),
                    version = "1",
                    scale = "4"
                })
                .Post("https://i.instagram.com/api/v1/qp/batch_fetch/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse IsEligibleForMonetizationProducts()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("product_types", "branded_content")
                .Get("https://i.instagram.com/api/v1/business/eligibility/is_eligible_for_monetization_products/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ShouldRequireProfessionalAccount()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/business/branded_content/should_require_professional_account/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse ProfileArchiveBadge()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _uuid = User.Uuid
                })
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .Post("https://i.instagram.com/api/v1/archive/reel/profile_archive_badge/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FundRaiser(string userId = null)
        {
            if (userId == null) userId = User.AccountId;

            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/fundraiser/{userId}/standalone_fundraiser_info/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse CanCreatePersonalFundraisers()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/fundraiser/can_create_personal_fundraisers/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FbRecentSearches()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/fbsearch/recent_searches/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse CommerceDestination()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("is_tab", "true")
                .Get($"https://i.instagram.com/api/v1/commerce/destination/prefetch/eligible/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse IgShopRecentSearches()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/fbsearch/ig_shop_recent_searches/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FbSearchNullStateDynamicSections()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("type", "blended")
                .Get($"https://i.instagram.com/api/v1/fbsearch/nullstate_dynamic_sections/")
                .ToResponse<TraitResponse>();
        }

        public ConsentResponse ConsentExistingUserFlow()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://b.i.instagram.com/api/v1/consent/existing_user_flow/ ")
                .ToResponse<ConsentResponse>();
        }

        public ConsentResponse ConsentDob()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    current_screen_key = "dob",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    day = Utils.Random.Next(1, 25),
                    month = Utils.Random.Next(1, 12),
                    year = Utils.Random.Next(1990, 2002)
                })
                .Post("https://b.i.instagram.com/api/v1/consent/existing_user_flow/")
                .ToResponse<ConsentResponse>();
        }

        public ConsentResponse ConsentQpIntro()
        {
            User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    current_screen_key = "qp_intro",
                    updates = "{\"existing_user_intro_state\":\"2\"}",
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://b.i.instagram.com/api/v1/consent/existing_user_flow/")
                .ToResponse<ConsentResponse>();

            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    current_screen_key = "tos",
                    updates = "{\"tos_data_policy_consent_state\":\"2\"}",
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post("https://b.i.instagram.com/api/v1/consent/existing_user_flow/")
                .ToResponse<ConsentResponse>();
        }

        public TraitResponse OpenPrivacyFlowCheckpoint()
        {
            return User.Request
                .AddCustomHeader("Upgrade-Insecure-Requests", "1")
                .AddCustomHeader("Sec-Fetch-Site", "none")
                .AddCustomHeader("Sec-Fetch-Mode", "navigate")
                .AddCustomHeader("Sec-Fetch-User", "?1")
                .AddCustomHeader("Sec-Fetch-Dest", "document")
                .Get("https://b.i.instagram.com/privacy/checks/?privacy_flow=1&next=instagram://checkpoint/dismiss")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse AgreePrivacyFlowCheckpoint()
        {
            return User.Request
                .AddCustomHeader("Upgrade-Insecure-Requests", "1")
                .AddCustomHeader("Sec-Fetch-Site", "none")
                .AddCustomHeader("Sec-Fetch-Mode", "navigate")
                .AddCustomHeader("Sec-Fetch-User", "?1")
                .AddCustomHeader("Sec-Fetch-Dest", "document")
                .Get("https://b.i.instagram.com/privacy/checks/?privacy_flow=1&next=instagram://checkpoint/dismiss")
                .ToResponse<ConsentResponse>();
        }

        public TraitResponse PrivacyAccept()
        {
            var tempCookies = User.Request.Cookies;
            var tempUseragent = User.Request.UserAgent;

            // Clear request data
            User.Request.UserAgent = $"Mozilla/5.0 (Linux; Android 9; SM-G965N Build/QP1A.190711.020; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/92.0.4515.131 Mobile Safari/537.36\" {User.GetUserAgent()}";
            User.Request.Cookies.Clear();

            User.Request.Cookies.Add("authorization", User.State.Authorization);
            if (!string.IsNullOrEmpty(User.GetToken()))
                User.Request.Cookies.Add("csrftoken", User.GetToken());
            User.Request.Cookies.Add("ds_user_id", User.State.IgUserId);
            User.Request.Cookies.Add("mid", User.State.Mid);
            User.Request.Cookies.Add("rur", User.State.IgRur);

            var cookieRur = string.Empty;
            var headerAuth = string.Empty;

            var response = User.Request
                .SetCustomRequest()
                .AddHeader("Host", "i.instagram.com")
                .AddHeader("Cache-Control", "max-age=0")
                .AddHeader("Upgrade-Insecure-Requests", "1")
                .AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
                .AddHeader("X-Requested-With", "com.instagram.android")
                .AddHeader("Sec-Fetch-Site", "none")
                .AddHeader("Sec-Fetch-Mode", "navigate")
                .AddHeader("Sec-Fetch-User", "?1")
                .AddHeader("Sec-Fetch-Dest", "document")
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", $"{Constants.AcceptLanguage.Replace(" ", "")};q=0.8")
                .Get("https://i.instagram.com/privacy/checks/?privacy_flow=1&next=instagram://checkpoint/dismiss");

            if (response.ContainsHeader("ig-set-ig-u-rur") && !string.IsNullOrEmpty(response["ig-set-ig-u-rur"]))
            {
                cookieRur = response["ig-set-ig-u-rur"];
            }

            if (response.ContainsHeader("ig-set-authorization") &&
                !response["ig-set-authorization"].EndsWith(":"))
            {
                headerAuth = response["ig-set-authorization"];
            }

            var responseString = response.ToString();

            string rolloutHash = Utils.TryParse(responseString, "(?<=rollout_hash.:.)(.+?)(?=\")");
            string csrfTokenWeb = Utils.TryParse(responseString, "(?<=csrf_token.:.)(.+?)(?=\")");
            Utils.RandomSleep(3000, 3500);

            if (!string.IsNullOrEmpty(cookieRur))
                User.Request.Cookies["rur"] = cookieRur;

            if (!string.IsNullOrEmpty(csrfTokenWeb))
                User.Request.Cookies["csrftoken"] = csrfTokenWeb;

            var resp = User.Request
                .SetCustomRequest()
                .AddHeader("Host", "i.instagram.com")
                .AddHeader("Content-Length", "null")
                .AddHeader("X-Mid", User.State.Mid)
                .AddHeader("X-Ig-Www-Claim", User.State.IgWwwClaim ?? "0")
                .AddHeader("X-Instagram-Ajax", rolloutHash)
                .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                .AddHeader("Accept", "*/*")
                .AddHeader("X-Requested-With", "XMLHttpRequest")
                .AddHeader("X-Asbd-Id", "198387")
                .AddHeader("X-Csrftoken", csrfTokenWeb)
                .AddHeader("X-Ig-App-Id", Constants.InstagramAppId)
                .AddHeader("Origin", "https://i.instagram.com")
                .AddHeader("Sec-Fetch-Site", "same-origin")
                .AddHeader("Sec-Fetch-Mode", "cors")
                .AddHeader("Sec-Fetch-Dest", "empty")
                .AddHeader("Referer", "https://i.instagram.com/privacy/checks/?privacy_flow=1&next=instagram://checkpoint/dismiss")
                .AddHeader("Accept-Encoding", "gzip, deflate")
                .AddHeader("Accept-Language", $"{Constants.AcceptLanguage.Replace(" ", "")};q=0.8")
                .AddParam("doc_id", "4181090201923535")
                .AddParam("variables", "{\"third_party_tracking_opt_in\":true,\"cross_site_tracking_opt_in\":true,\"input\":{\"client_mutation_id\":0}}")
                .Post("https://i.instagram.com/web/wwwgraphql/ig/query/")
                .ToResponse<TraitResponse>();

            User.Request.Cookies = tempCookies;
            User.Request.UserAgent = tempUseragent;
            return resp;
        }

        // Old

        public SignupConfigResponse ConsentGetSignupConfig()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("guid", User.Uuid)
                .Get("https://i.instagram.com/api/v1/consent/get_signup_config/")
                .ToResponse<SignupConfigResponse>();
        }

        public TraitResponse ConsentCheckAgeEligibility(int day, int month, int year)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("day", day)
                .AddParam("year", year)
                .AddParam("month", month)
                .Post("https://i.instagram.com/api/v1/consent/check_age_eligibility/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse NewUserFlowBegins()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    device_id = User.Uuid
                })
                .Post("https://i.instagram.com/api/v1/consent/new_user_flow_begins/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse AsyncGetNdxIgSteps()
        {
            return User.Request
                .AddDefaultHeaders()
                .Get("https://i.instagram.com/api/v1/devices/ndx/api/async_get_ndx_ig_steps/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetNotificationBadge()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("phone_id", User.PhoneId)
                .AddParam("user_ids", User.GetUserIdFromSession())
                .AddParam("device_id", User.Uuid)
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/notifications/badge/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse FbGetInviteSuggestions(bool first = true)
        {
            User.Request
                .AddDefaultHeaders()
                .AddParam("_uuid", User.Uuid);

            if (first)
            {
                User.Request
                    .AddParam("offset", "0")
                    .AddParam("count", "50");
            }
            else
            {
                User.Request
                    .AddParam("count_only", "1");
            }

            return User.Request.Post("https://i.instagram.com/api/v1/fb/get_invite_suggestions/")
                 .ToResponse<TraitResponse>();
        }
    }
}