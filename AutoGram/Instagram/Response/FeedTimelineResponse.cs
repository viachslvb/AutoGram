using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Response
{
    class FeedTimelineResponse : TraitResponse, IResponse
    {
        public int num_results;
        public bool more_available;
        public bool auto_load_more_enabled;
        public FeedItem[] feed_items;
        public string next_max_id;

        public bool HasResults => num_results > 0;
        public FeedItem GetFirstItem => feed_items.First();
        public string GetMaxId => next_max_id;
    }

    class FeedItem
    {
        public MediaOrAd media_or_ad;

        public string GetPk => media_or_ad.pk;
        public string GetId => media_or_ad.id;
    }

    class MediaOrAd
    {
        public string taken_at;
        public string pk;
        public string id;
        public string device_timestamp;
        public string media_type;
        public string code;
        public string client_cache_key;
    }
}
