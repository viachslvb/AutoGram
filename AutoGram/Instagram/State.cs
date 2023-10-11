using AutoGram.Instagram.Response;
using Newtonsoft.Json;

namespace AutoGram.Instagram
{
    class State
    {
        public string IgWwwClaim;
        public string Authorization;

        [JsonIgnore]
        public string PwKeyId;
        [JsonIgnore]
        public string PwPubKey;

        public string Mid;
        public string IgUserId;
        public string IgRur;
        public string IgIntentedUserId;
        public string IgDirectRegionHint;

        [JsonIgnore]
        public string IgNavChain;

        public void Update<T>(T response)
        {
            if (!(response is TraitResponse res)) return;
            if (res.HttpResponse == null) return;

            if (res.HttpResponse.ContainsHeader("x-ig-set-www-claim"))
                IgWwwClaim = res.HttpResponse["x-ig-set-www-claim"];

            if (res.HttpResponse.ContainsHeader("ig-set-authorization") &&
                !res.HttpResponse["ig-set-authorization"].EndsWith(":"))
                Authorization = res.HttpResponse["ig-set-authorization"];

            if (res.HttpResponse.ContainsHeader("ig-set-authorization") &&
                res.HttpResponse["ig-set-authorization"].EndsWith(":") &&
                !string.IsNullOrEmpty(Authorization))
            {
                Authorization = string.Empty;
            }

            if (res.HttpResponse.ContainsHeader("ig-set-password-encryption-key-id"))
                PwKeyId = res.HttpResponse["ig-set-password-encryption-key-id"];

            if (res.HttpResponse.ContainsHeader("ig-set-password-encryption-pub-key"))
                PwPubKey = res.HttpResponse["ig-set-password-encryption-pub-key"];

            if (res.HttpResponse.ContainsHeader("ig-set-x-mid"))
                Mid = res.HttpResponse["ig-set-x-mid"];

            if (res.HttpResponse.ContainsHeader("ig-set-ig-u-rur") && !string.IsNullOrEmpty(res.HttpResponse["ig-set-ig-u-rur"]))
                IgRur = res.HttpResponse["ig-set-ig-u-rur"];

            if (res.HttpResponse.ContainsHeader("ig-set-ig-u-ds-user-id") && !string.IsNullOrEmpty(res.HttpResponse["ig-set-ig-u-ds-user-id"]))
                IgUserId = res.HttpResponse["ig-set-ig-u-ds-user-id"];

            if (res.HttpResponse.ContainsHeader("ig-set-ig-u-ig-direct-region-hint") && !string.IsNullOrEmpty(res.HttpResponse["ig-set-ig-u-ig-direct-region-hint"]))
                IgDirectRegionHint = res.HttpResponse["ig-set-ig-u-ig-direct-region-hint"];
        }
    }
}
