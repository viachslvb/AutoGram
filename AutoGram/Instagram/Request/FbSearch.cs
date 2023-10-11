using System.Web;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;

namespace AutoGram.Instagram.Request
{
    class FbSearch : RequestCollection
    {
        public FbSearch(Instagram instagram) : base(instagram)
        {
        }

        public TopSearchResponse TopSearchFlat(string query, string context = "blended")
        {
            query = query.Contains("#")
                ? query
                : $"{HttpUtility.UrlEncode("#")}{query}";

            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("timezone_offset", User.TimezoneOffset)
                .AddUrlParam("count", "30")
                .AddUrlParam("query", Utils.EncodeNonAsciiCharacters(query))
                .AddUrlParam("context", context)
                .Get("https://i.instagram.com/api/v1/fbsearch/topsearch_flat/")
                .ToResponse<TopSearchResponse>();
        }
    }
}
