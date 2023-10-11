using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Model
{
    class Media
    {
        [JsonProperty("caption")]
        public Caption Caption;

        public string Pk;
        public string Id;
        public string Code;

        [JsonProperty("taken_at")] public string TakenAt;

        public bool IsCaption() => this.Caption != null;

        public string GetPk() => this.Pk;

        public string GetId() => this.Pk;

        public string GetCaption() => this.Caption?.Text;
    }
}
