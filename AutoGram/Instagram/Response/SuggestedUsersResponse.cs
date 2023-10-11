using System.Linq;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class SuggestedUsersResponse : TraitResponse, IResponse
    {
        [JsonProperty("more_available")]
        public bool MoreAvailable { get; set; }

        [JsonProperty("max_id")]
        public string MaxId { get; set; }

        [JsonProperty("suggested_users")]
        public SuggestedUsers SuggestedUsers;

        [JsonProperty("new_suggested_users")]
        public NewSuggestedUsers NewSuggestedUsers;

        public bool? IsSuggestedUsers =>
            SuggestedUsers?.Suggestions?.Any();

        public bool? IsNewSuggestedUsers =>
            NewSuggestedUsers?.Suggestions?.Any();

        public Suggestion[] GetSuggestedUsers => SuggestedUsers?.Suggestions;
        public Suggestion[] GetNewSuggestedUsers => NewSuggestedUsers?.Suggestions;
    }

    class SuggestedUsers
    {
        public Suggestion[] Suggestions { get; set; }
    }

    class NewSuggestedUsers
    {
        public Suggestion[] Suggestions { get; set; }
    }

    class Suggestion
    {
        public SuggestedUser User { get; set; }
    }

    class SuggestedUser : User
    {
        public bool Following { get; set; }

        [JsonProperty("incoming_request")]
        public bool IncomingRequest { get; set; }

        public ShowManyResponse.FriendshipStatus Status { get; set; }
    }
}
