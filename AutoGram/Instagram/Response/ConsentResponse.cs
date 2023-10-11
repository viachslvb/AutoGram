using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Response
{
    class ConsentResponse : TraitResponse, IResponse
    {
        [JsonProperty("screen_key")]
        public string ScreenKey { get; set; }

        public bool IsDobConsent => ScreenKey == "dob";

        public bool IsQpIntro => ScreenKey == "qp_intro";
        public bool IsFinished => ScreenKey == "finished";



    }
}
