using System.Collections.Generic;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class ChainingResponse : TraitResponse, IResponse
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; }
    }
}
