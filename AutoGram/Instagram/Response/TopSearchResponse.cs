using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoGram.Instagram.Response  
{
    class TopSearchResponse : TraitResponse, IResponse
    {
        [JsonProperty("list")]
        public List<JObject> List;

        public bool IsValidHashtag(string hashtag)
        {
            if (List.Any())
            {
                var firstPosition = List.FirstOrDefault();

                JToken res;
                if (firstPosition.TryGetValue("hashtag", out res))
                {
                    return hashtag == (string) res["name"];
                }
            }
            return false;
        }
    }
}
