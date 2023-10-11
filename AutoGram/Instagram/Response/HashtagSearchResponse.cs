using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoGram.Instagram.Response
{
    class HashtagSearchResponse : TraitResponse, IResponse
    {

        [JsonProperty("results")]
        public List<JObject> List;

        public bool IsValidHashtag(string hashtag)
        {
            if (List.Any())
            {
                var firstPosition = List.FirstOrDefault();

                JToken res;
                if (firstPosition.TryGetValue("name", out res))
                {
                    return hashtag == (string)res 
                        || Utils.EncodeNonAsciiCharacters(hashtag) == (string)res;
                }
            }
            return false;
        }
    }
}
