using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Model
{
    class Caption
    {
        [JsonProperty("pk")] public string Pk;

        [JsonProperty("user_id")] public string UserId;

        [JsonProperty("text")] public string Text;
    }
}
