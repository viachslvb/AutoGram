using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class LastSeenAtModel
    {
        [JsonProperty("timestamp")] public string Timestamp;

        [JsonProperty("item_id")] public string ItemId;
    }
}
