using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.CreateAccount
{
    class UsernameSuggestionResponse : TraitResponse, IResponse
    {
        [JsonProperty("available")] public bool IsAvailable { get; set; }
        [JsonProperty("username_suggestions")] public UsernameSuggestionsModel UsernameSuggestions { get; set; }

        public List<string> GetSuggestions()
        {
            if (UsernameSuggestions?.SuggestionsWithMetadata?.Suggestions != null)
            {
                if (UsernameSuggestions.SuggestionsWithMetadata.Suggestions.Any())
                {
                    return UsernameSuggestions.SuggestionsWithMetadata.Suggestions.Select(x => x.Username).ToList();
                }
            }

            return null;
        }
    }

    class UsernameSuggestionsModel
    {
        [JsonProperty("suggestions_with_metadata")] public SuggestionsWithMetadataModel SuggestionsWithMetadata { get; set; }
    }

    class SuggestionsWithMetadataModel
    {
        [JsonProperty("suggestions")] public List<SuggestionModel> Suggestions { get; set; }
    }

    class SuggestionModel
    {
        [JsonProperty("username")] public string Username { get; set; }
    }

}
