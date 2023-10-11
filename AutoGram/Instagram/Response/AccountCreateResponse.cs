using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class AccountCreateResponse : TraitResponse, IResponse
    {
        public bool account_created;

        [JsonProperty("created_user")]
        public Model.User User;

        public bool IsCreated => account_created;

        public bool IsSpam() => IsMessage() && GetMessage().Contains("feedback_required");
    }
}
