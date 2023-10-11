using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Stories
{
    class HighlightsResponse : TraitResponse, IResponse
    {
        [JsonProperty("reels")] public Dictionary<string, HighlightsOwner> HighlightsOwners;

        public bool IsValid => HighlightsOwners.Any()
                               && HighlightsOwners.FirstOrDefault().Value.Stories != null
                               && HighlightsOwners.FirstOrDefault().Value.Stories.Any();

        public UserStory GetLatestStory()
        {
            if (!IsValid) return null;

            var latestReelMedia = HighlightsOwners.FirstOrDefault().Value.LatestReelMedia;

            return HighlightsOwners.FirstOrDefault().Value.Stories.FirstOrDefault(s => s.TakenAt == latestReelMedia);
        }
    }
}
