using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Friendship
{
    class FriendshipsResponse : TraitResponse, IResponse
    {
        [JsonProperty("users")] public List<FriendshipUserModel> Users;
        [JsonProperty("next_max_id")] public string NextMaxId;

        public bool IsNextMaxId => !string.IsNullOrEmpty(NextMaxId);
    }
}
