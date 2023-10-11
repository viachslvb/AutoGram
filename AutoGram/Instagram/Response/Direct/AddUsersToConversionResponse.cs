using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class AddUsersToConversionResponse : TraitResponse, IResponse
    {
        public bool IsFailedUsers => Error.GroupReachabilityError != null && Error.GroupReachabilityError.FailedUserIds.Count > 0;
        public bool IsGroupReachabilityError => Error.ErrorType == "group_reachability_error";

        [JsonProperty("error")] public ResponseError Error;

        public List<string> FailedUserIds => Error.GroupReachabilityError.FailedUserIds;
    }

    class ResponseError
    {
        [JsonProperty("error_type")] public string ErrorType;
        [JsonProperty("group_reachability_error")]
        public GroupReachabilityError GroupReachabilityError;
    }

    class GroupReachabilityError
    {
        [JsonProperty("failed_user_ids")] public List<string> FailedUserIds;
    }
}
