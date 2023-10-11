using Newtonsoft.Json;

namespace Database
{
    public class InstagramApp
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string SignatureKey { get; set; }
        public string Capabilities { get; set; }
        public string BloksVersionId { get; set; }

        [JsonIgnore]
        public AndroidDevice AndroidDevice { get; set; }
    }
}
