using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGram.Instagram.Response.Stories;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Model
{
    class UserStoriesResponse : TraitResponse, IResponse
    {
        [JsonProperty("items")] public List<UserStory> Items;
        [JsonProperty("media_count")] public int MediaCount;

        [JsonIgnore] public bool IsStories => Items != null && Items.Any();
    }
}
