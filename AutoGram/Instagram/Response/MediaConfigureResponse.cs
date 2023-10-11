using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class MediaConfigureResponse : TraitResponse, IResponse
    {
        [JsonProperty("media")]
        public Model.Media Media;

        [JsonProperty("upload_id")]
        public string UploadId;

        [JsonProperty("client_sidecar_id")]
        public string SidecarId;
    }
}
