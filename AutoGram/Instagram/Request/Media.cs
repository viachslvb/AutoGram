using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Friendship;

namespace AutoGram.Instagram.Request
{
    class Media : RequestCollection
    {
        public Media(Instagram instagram) : base(instagram)
        {
        }

        public MediaCommentsResponse GetComments(string mediaId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("can_support_threading", "true")
                .Get($"https://i.instagram.com/api/v1/media/{mediaId}/comments/")
                .ToResponse<MediaCommentsResponse>();
        }

        public TraitResponse LikeComment(string pk)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .Post($"https://i.instagram.com/api/v1/media/{pk}/comment_like/")
                .ToResponse<TraitResponse>();
        }

        public MediaCommentResponse Comment(string mediaId, string comment)
        {
            var data = Utils.GenerateUserBreadcrumb(comment.Length);

            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    user_breadcrumb = Utils.Base64Encode(Utils.GetSHA256(data, User.App.SignatureKey)) + "\n" + Utils.Base64Encode(data) + "\n",
                    idempotence_token = Utils.GenerateUUID(true),
                    _csrftoken = User.GetToken(),
                    radio_type = "wifi-none",
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    comment_text = Utils.EncodeNonAsciiCharacters(comment),
                    containermodule = "self_comments_feed_timeline"
                }, true)
                .Post($"https://i.instagram.com/api/v1/media/{mediaId}/comment/")
                .ToResponse<MediaCommentResponse>();
        }

        public TraitResponse DisableComments(string mediaId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post($"https://i.instagram.com/api/v1/media/{mediaId}/disable_comments/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse Like(string mediaId, string username, string userId, string moduleName = "profile")
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    module_name = moduleName,
                    media_id = mediaId,
                    _csrftoken = User.GetToken(),
                    username,
                    user_id = userId,
                    radio_type = "wifi-none",
                    _uid = User.AccountId,
                    _uuid = User.Uuid
                })
                .AddParam("d", "1")
                .Post($"https://i.instagram.com/api/v1/media/{mediaId}/like/")
                .ToResponse<TraitResponse>();
        }

        public FriendshipsResponse GetLikers(string mediaId)
        {
            return User.Request
                .AddDefaultHeaders()
                .Get($"https://i.instagram.com/api/v1/media/{mediaId}/likers/")
                .ToResponse<FriendshipsResponse>();
        }
    }
}
