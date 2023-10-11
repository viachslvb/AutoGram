using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response
{
    class HashtagMediaResponse : TraitResponse, IResponse
    {
        [JsonProperty("sections")]
        public List<SectionMediaResponse> Sections { get; set; } = new List<SectionMediaResponse>();

        [JsonProperty("more_available")]
        public bool MoreAvailable { get; set; }

        [JsonProperty("next_max_id")]
        public string NextMaxId { get; set; }

        [JsonProperty("next_page")]
        public int? NextPage { get; set; }

        [JsonProperty("next_media_ids")]
        public List<long> NextMediaIds { get; set; }

        [JsonProperty("auto_load_more_enabled")]
        public bool? AutoLoadMoreEnabled { get; set; }
    }

    class SectionMediaResponse
    {
        [JsonProperty("layout_type")]
        public string LayoutType { get; set; }

        [JsonProperty("layout_content")]
        public SectionMediaLayoutContentResponse LayoutContent { get; set; }

        [JsonProperty("feed_type")]
        public string FeedType { get; set; }

        [JsonProperty("explore_item_info")]
        public SectionMediaExploreItemInfoResponse ExploreItemInfo { get; set; }
    }

    class SectionMediaExploreItemInfoResponse
    {
        [JsonProperty("num_columns")]
        public int NumBolumns { get; set; }

        [JsonProperty("total_num_columns")]
        public int TotalNumBolumns { get; set; }

        [JsonProperty("aspect_ratio")]
        public float AspectRatio { get; set; }

        [JsonProperty("autoplay")]
        public bool Autoplay { get; set; }
    }

    class SectionMediaLayoutContentResponse
    {
        [JsonProperty("medias")]
        public List<MediaAlbumResponse> Medias { get; set; }
    }

    class MediaAlbumResponse
    {
        [JsonProperty("media")] public MediaItem Media { get; set; }

        [JsonProperty("client_sidecar_id")] public string ClientSidecarId { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}
