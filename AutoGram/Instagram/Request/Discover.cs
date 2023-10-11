using System.Collections.Generic;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;

namespace AutoGram.Instagram.Request
{
    class Discover : RequestCollection
    {
        public Discover(Instagram instagram) : base(instagram)
        {
        }

        public SuggestedUsersResponse GetSuggestedUsers(string module = "discover_people", string maxId = null)
        {
            User.Request
                .AddDefaultHeaders()
                .AddParam("phone_id", User.PhoneId);

            if (maxId != null)
            {
                User.Request
                    .AddParam("max_id", maxId);
            }

            return User.Request
                .AddParam("module", module)
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/discover/ayml/")
                .ToResponse<SuggestedUsersResponse>();
        }

        public void MarksUSeen()
        {
            User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/discover/mark_su_seen/");
        }

        public ChainingResponse Chaining(string targetId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("target_id", targetId)
                .Get($"https://i.instagram.com/api/v1/discover/chaining/")
                .ToResponse<ChainingResponse>();
        }

        public TraitResponse Explore(bool isPrefetch = false, long maxId = 0, string module = "explore_popular")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("is_prefetch", isPrefetch)
                .AddUrlParam("max_id", maxId)
                .AddUrlParam("module", module)
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .AddUrlParam("session_id", User.SessionId)
                .Get("https://i.instagram.com/api/v1/discover/explore/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse TopicalExplore()
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("is_prefetch", "false")
                .AddUrlParam("omit_cover_media", "true")
                .AddUrlParam("module", "explore_popular")
                .AddUrlParam("reels_configuration", "default")
                .AddUrlParam("use_sectional_payload", "true")
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .AddUrlParam("session_id", User.SessionId)
                .AddUrlParam("include_fixed_destinations", "true")
                .Get("https://i.instagram.com/api/v1/discover/topical_explore/")
                .ToResponse<TraitResponse>();
        }
    }
}
