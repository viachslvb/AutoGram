using System.Linq;
using System.Text;
using AutoGram.Helpers;
using AutoGram.Instagram.Requests;
using AutoGram.Instagram.Response;
using AutoGram.Instagram.Response.Direct;

namespace AutoGram.Instagram.Request
{
    class Direct : RequestCollection
    {
        public Direct(Instagram instagram) : base(instagram)
        {
        }

        public InboxResponse Inbox(string cursor = null, string direction = null)
        {
            User.Request
                .AddDefaultHeaders()
                .AddUrlParam("persistentBadging", "true")
                .AddUrlParam("use_unified_inbox", "true");

            if (!string.IsNullOrEmpty(cursor) && !string.IsNullOrEmpty(direction))
            {
                User.Request
                    .AddUrlParam("cursor", cursor)
                    .AddUrlParam("direction", direction);
            }

            return User.Request
                .Get("https://i.instagram.com/api/v1/direct_v2/inbox/")
                .ToResponse<InboxResponse>();
        }

        public InboxResponse PendingInbox(string cursor = null, string direction = null)
        {
            User.Request
                .AddDefaultHeaders()
                .AddUrlParam("persistentBadging", "true")
                .AddUrlParam("use_unified_inbox", "true");

            if (!string.IsNullOrEmpty(cursor) && !string.IsNullOrEmpty(direction))
            {
                User.Request
                    .AddUrlParam("cursor", cursor)
                    .AddUrlParam("direction", direction);
            }

            return User.Request
                .Get("https://i.instagram.com/api/v1/direct_v2/pending_inbox/")
                .ToResponse<InboxResponse>();
        }

        public ThreadResponse OpenThread(string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("visual_message_return_type", "unseen")
                .Get(
                    $"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/")
                .ToResponse<ThreadResponse>();
        }

        public TraitResponse AcceptRequest(string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post(
                    $"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/approve/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse DeclineRequest(string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/decline/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse MarkSeen(string threadId, string itemId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("thread_id", threadId)
                .AddParam("action", "mark_seen")
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .AddParam("use_unified_inbox", "true")
                .AddParam("item_id", itemId)
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/items/{itemId}/seen/")
                .ToResponse<TraitResponse>();
        }

        public SendMessageResponse SendText(string text, string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("thread_ids", $"[{threadId}]")
                .AddParam("action", "send_item")
                .AddParam("client_context", Utils.GenerateUUID(true))
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("text", text)
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/text/")
                .ToResponse<SendMessageResponse>();
        }

        public SendMessageResponse SendTextToRecipientUsers(string text, string recipients)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("recipient_users", $"[[{recipients}]]")
                .AddParam("action", "send_item")
                .AddParam("client_context", Utils.GenerateUUID(true))
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("text", text)
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/text/")
                .ToResponse<SendMessageResponse>();
        }

        public SendMessageResponse SendLinkToRecipientUsers(string text, string link, string recipients)
        {
            string clientContext = Utils.GenerateClientContextToken();

            return User.Request
                .AddDefaultHeaders()
                .AddParam("recipient_users", $"[[{recipients}]]")
                .AddParam("link_text", text ?? string.Empty)
                .AddParam("link_urls", $"[{new[] { link }.EncodeList()}]")
                .AddParam("action", "send_item")
                .AddParam("is_shh_mode", "0")
                .AddParam("send_attribution", "inbox_new_message")
                .AddParam("client_context", clientContext)
                .AddParam("device_id", User.DeviceId)
                .AddParam("mutation_token", clientContext)
                .AddParam("_uuid", User.Uuid)
                .AddParam("nav_chain", User.State.IgNavChain)
                .AddParam("offline_threading_id", clientContext)
                .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/link/")
                .ToResponse<SendMessageResponse>();
        }

        public TraitResponse UpdateDirectThreadTitle(string threadId, string title)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("_uuid", User.Uuid)
                .AddParam("title", title)
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/update_title/")
                .ToResponse<TraitResponse>();
        }

        public SendMessageResponse SendLink(string text, string link, string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("link_text", text ?? string.Empty)
                .AddParam("link_urls", $"[{new[] { link }.EncodeList()}]")
                .AddParam("action", "send_item")
                .AddParam("thread_ids", $"[{threadId}]")
                .AddParam("client_context", Utils.GenerateUUID(true))
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/link/")
                .ToResponse<SendMessageResponse>();
        }

        public AddUsersToConversionResponse AddParticipantsToThread(string threadId, string participants)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("user_ids", $"[{participants}]")
                .AddParam("_uuid", User.Uuid)
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/{threadId}/add_user/")
                .ToResponse<AddUsersToConversionResponse>();
        }

        public SendMessageResponse SendLike(string threadId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("action", "send_item")
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .AddParam("thread_id", $"{threadId}")
                .AddParam("client_context", Utils.GenerateUUID(true))
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/broadcast/like/")
                .ToResponse<SendMessageResponse>();
        }

        public TraitResponse CreateGroupThread(string recipients, string threadTitle)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uuid = User.Uuid,
                    _uid = User.AccountId,
                    recipient_users = $"[[{recipients}]]",
                    thread_title = threadTitle
                })
                .Post($"https://i.instagram.com/api/v1/direct_v2/create_group_thread/")
                .ToResponse<TraitResponse>();
        }

        public SendMessageResponse LikeThreadMessage(string threadId, string itemId)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddParam("item_type", "reaction")
                .AddParam("reaction_type", "like")
                .AddParam("action", "send_item")
                .AddParam("_csrftoken", User.GetToken())
                .AddParam("_uuid", User.Uuid)
                .AddParam("thread_ids", $"[{threadId}]")
                .AddParam("client_context", Utils.GenerateUUID(true))
                .AddParam("node_type", "item")
                .AddParam("reaction_status", "created")
                .AddParam("item_id", itemId)
                .Post($"https://i.instagram.com/api/v1/direct_v2/threads/broadcast/reaction/")
                .ToResponse<SendMessageResponse>();
        }

        public VisualThreadItemsResponse ReturnUnseenVisualThreadItems(string threadId, string cursor)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddUrlParam("visual_message_return_type", "unseen")
                .AddUrlParam("visual_message_backward_limit", "0")
                .AddUrlParam("cursor", cursor)
                .AddUrlParam("visual_message_total_limit", "0")
                .AddUrlParam("visual_message_forward_limit", "0")
                .Get($"https://i.instagram.com/api/v1/direct_v2/visual_threads/{threadId}/")
                .ToResponse<VisualThreadItemsResponse>();
        }

        public TraitResponse MarkSeenVisualThreadItem(string threadId, string threadItem)
        {
            return User.Request
                .AddDefaultHeaders()
                .AddSignedParams(new
                {
                    _csrftoken = User.GetToken(),
                    _uid = User.AccountId,
                    _uuid = User.Uuid,
                    item_ids = $"[{threadItem}]"
                })
                .Post(
                    $"https://i.instagram.com/api/v1/direct_v2/visual_threads/{threadId}/item_seen/")
                .ToResponse<TraitResponse>();
        }

        public TraitResponse SendDisappearingPhoto(Photo photo, string threadId)
        {
            return User.Internal.UploadPhoto(photo, isDirect: true, threadId: threadId);
        }

        public TraitResponse FindParticipant(string query)
        {
            return User.Request
                .AddDefaultHeaders()
                .Get(
                    $"https://i.instagram.com/api/v1/direct_v2/ranked_recipients/?mode=reshare&show_threads=false&query={query}&use_unified_inbox=true")
                .ToResponse<TraitResponse>();
        }

        public SendMessageResponse SendPhoto(Photo photo, string threadId)
        {
            string boundary = Utils.GenerateBoundary();

            var sb = new StringBuilder();
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"action\"");
            sb.Append("\r\n\r\n");
            sb.Append("send_item");
            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"thread_ids\"");
            sb.Append("\r\n\r\n");
            sb.Append($"[{threadId}]");
            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");

            //sb.Append("Content-Disposition: form-data; name=\"configure_mode\"");
            //sb.Append("\r\n\r\n");
            //sb.Append($"2");
            //sb.Append("\r\n");
            //sb.Append("--" + boundary);
            //sb.Append("\r\n");

            //sb.Append("Content-Disposition: form-data; name=\"view_mode\"");
            //sb.Append("\r\n\r\n");
            //sb.Append($"replayable");
            //sb.Append("\r\n");
            //sb.Append("--" + boundary);
            //sb.Append("\r\n");

            sb.Append("Content-Disposition: form-data; name=\"client_context\"");
            sb.Append("\r\n\r\n");
            sb.Append(Utils.GenerateUUID(true));
            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"_csrftoken\"");
            sb.Append("\r\n\r\n");
            sb.Append(User.GetToken());
            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"_uuid\"");
            sb.Append("\r\n\r\n");
            sb.Append(User.Uuid);
            sb.Append("\r\n");
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append($"Content-Disposition: form-data; name=\"photo\"; filename=\"direct_temp_photo_{Utils.GenerateUploadId()}.jpg\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: application/octet-stream");
            sb.Append("\r\n");
            sb.Append("Content-Transfer-Encoding: binary");
            sb.Append("\r\n\r\n");

            byte[] a = Encoding.UTF8.GetBytes(sb.ToString());
            sb.Clear();

            byte[] b = photo.Image;

            sb.Append("\r\n--" + boundary + "--\r\n");
            byte[] c = Encoding.UTF8.GetBytes(sb.ToString());

            byte[] r = a.Concat(b).Concat(c).ToArray();

            return User.Request
                .AddDefaultHeaders()
                .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/upload_photo/", r,
                    "multipart/form-data; boundary=" + boundary)
                .ToResponse<SendMessageResponse>();
        }
    }
}
