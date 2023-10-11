using System;
using Database;
using AutoGram.Instagram.Devices;
using Newtonsoft.Json;
using xNet;

namespace AutoGram.Instagram.Request
{
    class Request : HttpRequest
    {
        private InstagramApp _app;
        private Device _device;
        private Instagram _user;

        public Request AddSignedParams(Object paramsToSign, bool unicode = false)
        {
            string jsonParams = !unicode ? JsonConvert.SerializeObject(paramsToSign)
                                         : JsonConvert.SerializeObject(paramsToSign).Replace("\\\\u", "\\u")
                                         .Replace("0.0,0.0", "0.0,-0.0")
                                         .Replace("\"null\"", "null");

            AddParam("signed_body", $"SIGNATURE.{jsonParams}");

            return this;
        }

        public HttpRequest AddUrlParamDecode(string key, string value)
        {
            value = value.Replace("[", "%5B")
                .Replace("]", "%5D")
                .Replace(",", "%2C")
                .Replace(":", "%3A");

            return AddUrlParam(key, value);
        }

        public Request AddDefaultHeaders(string prefetchRequest = null)
        {
            AddHeader("X-IG-Timezone-Offset", _user.TimezoneOffset);
            AddHeader("X-IG-Connection-Type", Constants.InstagramConnectionType);
            AddHeader("X-IG-Connection-Speed", $"{Utils.Random.Next(500, 3700)}kbps");
            AddHeader("X-IG-Device-ID", _user.Uuid);
            AddHeader("X-FB-HTTP-Engine", Constants.FacebookEngine);
            AddHeader("X-FB-Client-IP", "True");
            AddHeader("X-FB-Server-Cluster", "True");
            AddHeader("X-IG-App-Startup-Country", _device.GetUserAgentLocale.Split('_')[1]);
            AddHeader("X-IG-Mapped-Locale", _device.GetUserAgentLocale);

            // IG-INTENDED-USER-ID
            if (!string.IsNullOrEmpty(_user.State.IgIntentedUserId))
                AddHeader("IG-INTENDED-USER-ID", _user.State.IgIntentedUserId);

            AddHeader("X-IG-App-Locale", _device.GetUserAgentLocale);
            AddHeader("X-IG-Device-Locale", _device.GetUserAgentLocale);
            AddHeader("X-IG-Android-ID", _user.DeviceId);
            AddHeader("X-IG-App-ID", Constants.InstagramAppId);
            AddHeader("X-IG-Capabilities", _app.Capabilities);
            AddHeader("X-Bloks-Version-Id", _app.BloksVersionId);
            AddHeader("X-Bloks-Is-Layout-RTL", "false");
            AddHeader("X-Bloks-Is-Panorama-Enabled", "true");

            // IG NAV CHAIN
            if (!string.IsNullOrEmpty(_user.State.IgNavChain))
            {
                AddHeader("X-IG-Nav-Chain", _user.State.IgNavChain);
            }

            AddHeader("X-Pigeon-Session-Id", _user.PigeonSessionId);
            AddHeader("X-Pigeon-Rawclienttime", Utils.DateTimeNowTotalSecondsWithMs);
            AddHeader("X-IG-Bandwidth-Speed-KBPS", "-1.000");

            // AUTHORIZATION
            if (!string.IsNullOrEmpty(_user.State.Authorization))
                AddHeader("Authorization", _user.State.Authorization);

            // X-IG-WWW-Claim
            AddHeader("X-IG-WWW-Claim", _user.State.IgWwwClaim ?? "0");

            // IG-U-DS-USER-ID
            if (!string.IsNullOrEmpty(_user.State.IgUserId))
                AddHeader("IG-U-DS-USER-ID", _user.State.IgUserId);

            // IG-U-RUR
            if (!string.IsNullOrEmpty(_user.State.IgRur))
                AddHeader("IG-U-RUR", _user.State.IgRur);

            // IG-U-IG-DIRECT-REGION-HINT
            if (!string.IsNullOrEmpty(_user.State.IgDirectRegionHint))
                AddHeader("IG-U-IG-DIRECT-REGION-HINT", _user.State.IgDirectRegionHint);

            // X-MID
            if (!string.IsNullOrEmpty(_user.State.Mid))
                AddHeader("X-MID", _user.State.Mid);

            return this;
        }

        public Request AddCustomHeader(string key, string value)
        {
            AddHeader(key, value);

            return this;
        }

        public Request SetCustomRequest()
        {
            AddHeader("Custom", "True");

            return this;
        }

        public void SetApp(InstagramApp app)
        {
            _app = app;
        }

        public void SetDevice(Device device)
        {
            _device = device;
        }

        public void SetUser(Instagram user)
        {
            _user = user;
        }
    }
}
