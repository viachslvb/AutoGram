using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class FriendshipCreateResponse : TraitResponse, IResponse
    {
        [JsonProperty("friendship_status")]
        public FriendshipStatus FriendshipCreateStatus { get; set; }

        public class FriendshipStatus
        {
            public bool Following { get; set; }

            [JsonProperty("followed_by")]
            public bool FollowedBy { get; set; }

            public bool Blocking { get; set; }
            public bool Muting { get; set; }

            [JsonProperty("is_private")]
            public bool IsPrivate { get; set; }

            [JsonProperty("incoming_request")]
            public bool IncomingRequest { get; set; }

            [JsonProperty("outgoing_request")]
            public bool OutgoingRequest { get; set; }

            [JsonProperty("is_bestie")]
            public bool IsBestie { get; set; }
        }
    }
}
