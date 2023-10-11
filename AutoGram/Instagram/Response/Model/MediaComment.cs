using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AutoGram.Instagram.Response.Model
{
    class MediaComment
    {
        [JsonProperty("pk")]
        public string Pk { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt;

        [JsonProperty("comment_like_count")]
        public long CommentLikeCount { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}
