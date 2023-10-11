using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AutoGram.Instagram.Response
{
    class UserResponse : TraitResponse, IResponse
    {
        //[JsonProperty("message")]
        //public ErrorsMessage Message;

        public string GetMessage()
        {
            return this.Message != null ? this.Message : "Undefined error.";
        }
        
        //public string GetMessage()
        //{
        //    return this.Message.Errors.Any() ? this.Message.Errors[0] : "Undefined error.";
        //}

        public bool IsMessage()
        {
            return this.Message != null;
        }

        [JsonProperty("user")]
        public Model.User User;

        public bool IsInvalidDescription()
        {
            return this.GetMessage().Contains("invalid");
        }
    }

    class ErrorsMessage
    {
        public List<string> Errors;
    }
}
