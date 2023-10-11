using System.Collections.Generic;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class MediaCommentsResponse : TraitResponse, IResponse
    {
        [JsonProperty("comments")]
        public List<MediaComment> Comments { get; set; }
    }
}
