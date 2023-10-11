using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Friendship;

namespace AutoGram.Instagram.Request
{
    class FriendShips : RequestCollection
    {
        public FriendShips(Instagram instagram) : base(instagram)
        {
        }

        public ShowManyResponse ShowMany(string userIds)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("user_ids", userIds)
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/friendships/show_many/")
                .ToResponse<ShowManyResponse>();
        }

        public FriendshipCreateResponse.FriendshipStatus Show(string userId)
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/friendships/show/{userId}/")
                .ToResponse<FriendshipCreateResponse.FriendshipStatus>();
        }

        public TraitResponse MarkUserOverage(string userId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    user_id = userId,
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/friendships/mark_user_overage/{userId}/feed/")
                .ToResponse<TraitResponse>();
        }

        public FriendshipsResponse GetFollowers(string userId, string rankToken, string nextMaxId = "")
        {
            User.Request
                .AddDefaultHeaders();

            User.Request
                .AddUrlParam("search_surface", "follow_list_page");

            if (!string.IsNullOrEmpty(nextMaxId))
                User.Request
                    .AddUrlParam("max_id", nextMaxId);

            User.Request
                .AddUrlParam("query", "")
                .AddUrlParam("rank_token", rankToken);

            return User.Request
                .Get($"https://i.instagram.com/api/v1/friendships/{userId}/followers/")
                .ToResponse<FriendshipsResponse>();
        }

        public FriendshipsResponse GetFollowings(string userId, string rankToken, string nextMaxId = "")
        {
            User.Request
                .AddDefaultHeaders();

            if (!string.IsNullOrEmpty(nextMaxId))
                User.Request
                    .AddUrlParam("max_id", nextMaxId);

            User.Request
                .AddUrlParam("rank_token", Utils.GenerateUUID(true));

            return User.Request
                .Get($"https://i.instagram.com/api/v1/friendships/{userId}/following/")
                .ToResponse<FriendshipsResponse>();
        }

        public FriendshipCreateResponse Create(string userId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    user_id = userId,
                    radio_type = "wifi-none",
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/friendships/create/{userId}/")
                .ToResponse<FriendshipCreateResponse>();
        }
    }
}
