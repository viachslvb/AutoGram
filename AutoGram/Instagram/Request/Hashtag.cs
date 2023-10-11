using System.Web;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;

namespace AutoGram.Instagram.Request
{
    class Hashtag : RequestCollection
    {
        public Hashtag(Instagram instagram) : base(instagram)
        {
        }

        public void OpenHashtagPage(string hashtag)
        {
            User.Do(() => GetStory(hashtag));
            Utils.RandomSleep(500,1500);
            User.Do(() => GetInfo(hashtag));
        }

        public TraitResponse GetStory(string hashtag)
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/tags/{hashtag}/story/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse GetInfo(string hashtag)
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/tags/{hashtag}/info/")
                .ToResponse<TraitResponse>();
        }

        public HashtagMediaResponse GetPopularFeed(string hashtag, string rankToken)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("rank_token", rankToken)
                .Get($"https://i.instagram.com/api/v1/tags/{hashtag}/ranked_sections/")
                .ToResponse<HashtagMediaResponse>();
        }

        public HashtagMediaResponse GetRecentFeed(string hashtag, string rankToken)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("rank_token", rankToken)
                .Get($"https://i.instagram.com/api/v1/tags/{hashtag}/recent_sections/")
                .ToResponse<HashtagMediaResponse>();
        }

        public HashtagSearchResponse Search(string hashtag)
        {
            hashtag = $"{HttpUtility.UrlEncode("#")}{hashtag}";

            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .AddUrlParam("q", hashtag)
                .AddUrlParam("count", "30")
                .Get("https://i.instagram.com/api/v1/tags/search/")
                .ToResponse<HashtagSearchResponse>();
        }
    }
}
