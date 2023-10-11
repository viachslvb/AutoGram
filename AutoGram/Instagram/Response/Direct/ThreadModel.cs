using System.Collections.Generic;
using System.Linq;
using AutoGram.Instagram.Response.Model;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Response.Direct
{
    class ThreadModel
    {
        [JsonProperty("thread_id")] public string ThreadId;

        [JsonProperty("thread_v2_id")] public string ThreadV2Id;

        [JsonProperty("users")] public List<User> Users;

        [JsonProperty("items")] public List<ThreadItemModel> Items;

        [JsonProperty("last_activity_at")] public long LastActivityAt;

        [JsonProperty("inviter")] public User Inviter;

        [JsonProperty("pending")] public bool Pending;

        [JsonProperty("last_permanent_item")] public ThreadItemModel LastPermanentItem;

        [JsonProperty("last_seen_at")] public Dictionary<string, LastSeenAtModel> LastSeenAt;

        [JsonProperty("has_older")] public bool HasOlder;

        [JsonProperty("has_newer")] public bool HasNewer;

        [JsonProperty("newest_cursor")] public string NewestCursor;

        [JsonProperty("oldest_cursor")] public string OldestCursor;

        [JsonProperty("direct_story")] public DirectStoryModel DirectStory;

        [JsonIgnore]
        public bool IsUnseenDirectStory => DirectStory?.UnseenCount > 0;

        [JsonIgnore]
        public string Username => Users != null && Users.Any()
            ? Users.FirstOrDefault().Username
            : "Unknown user";

        public bool IsUnseenThread(string accountId)
        {
            return IsUnreedMessages(accountId) || IsUnseenDirectStory;
        }

        public bool IsUnreedMessages(string accountId)
        {
            if (LastSeenAt != null)
            {
                if (LastSeenAt.ContainsKey(accountId))
                {
                    var lastSeenTimestamp = LastSeenAt.FirstOrDefault(x => x.Key == accountId).Value.Timestamp;

                    if (LastPermanentItem == null)
                        return false;

                    return lastSeenTimestamp != LastPermanentItem.Timestamp.ToString();
                }
            }

            return true;
        }
    }
}
