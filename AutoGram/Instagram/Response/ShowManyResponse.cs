using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class ShowManyResponse : TraitResponse, IResponse
    {
        [JsonProperty("friendship_statuses")]
        public Dictionary<string, FriendshipStatus> Users { get; set; }

        public class FriendshipStatus
        {
            public bool Following { get; set; }

            [JsonProperty("is_private")]
            public bool IsPrivate { get; set; }

            [JsonProperty("incoming_request")]
            public bool IncomingRequest { get; set; }

            [JsonProperty("outgoing_request")]
            public bool OutComingRequest { get; set; }

            [JsonProperty("is_bestie")]
            public bool IsBestie { get; set; }
        }
    }
}
