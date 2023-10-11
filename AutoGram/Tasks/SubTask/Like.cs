using AutoGram.Instagram.Response.Model;

namespace AutoGram.Task.SubTask
{
    static class Like
    {
        public static void Media(Instagram.Instagram user, User targetUser, string mediaId, string fromModule = "profile")
        {
            if (user.LiveSettings.Like.IsLimit)
                return;

            var response = user.Do(() => user.Media.Like(mediaId, targetUser.Username, targetUser.Pk, fromModule));

            if (response.IsOk())
            {
                user.Log($"Liked post ID #{mediaId}" +
                                        $" of {targetUser.Username}.");

                user.Activity.Actions.Likes++;
            }
            else
            {
                string errorMessage = response.IsMessage()
                    ? response.GetMessage()
                    : "Like error.";

                Log.Write(errorMessage, LogResource.Live);
                user.Log(errorMessage);
            }

            if (user.Activity.Actions.Likes >= user.LiveSettings.Like.Limit)
            {
                user.LiveSettings.Like.IsLimit = true;
            }
        }

        public static void Comment(Instagram.Instagram user, string pk)
        {
            if (user.LiveSettings.Like.IsLimit)
                return;

            var response = user.Do(() => user.Media.LikeComment(pk));

            if (response.IsOk())
            {
                user.Log($"Liked comment ID #{pk}");

                user.Activity.Actions.Likes++;
            }
            else
            {
                string errorMessage = response.IsMessage()
                    ? response.GetMessage()
                    : "Like error.";

                Log.Write(errorMessage, LogResource.Live);
                user.Log(errorMessage);
            }

            if (user.Activity.Actions.Likes >= user.LiveSettings.Like.Limit)
            {
                user.LiveSettings.Like.IsLimit = true;
            }
        }
    }
}
