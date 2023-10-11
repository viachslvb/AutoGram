using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Model
{
    class User
    {
        [JsonProperty("Is_private")]
        public bool IsPrivate;

        public string Pk;
        public string Email;
        public string Full_name;

        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }

        public string Username;
        public string Gender;
        public string Phone_number;
        public string Biography;
        public string External_url;
        public string Profile_pic_url;
        public int Follower_count;
    }
}
