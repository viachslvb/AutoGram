using System.Threading;

namespace AutoGram.Task.SubTask
{
    static class AddComment
    {
        public static void Do(Instagram.Instagram user, string mediaId, string comment)
        {
            var response = user.Do(() => user.Media.Comment(mediaId, comment));

            if (response.IsOk())
            {
                user.Log("Сommented on.");

                if (!response.IsComment())
                {
                    user.Log("But the comment was automatically deleted.");
                }

                if (Settings.Advanced.Post.Content.Comment.HideComment)
                {
                    Thread.Sleep(2000);

                    user.Do(() => user.Media.DisableComments(mediaId));
                }
            }
            else
            {
                if (response.IsSpam())
                {
                    user.Log("Commenting failed.");
                    return;
                }

                string errorMessage = response.IsMessage()
                    ? response.GetMessage()
                    : "Commenting failed. Undefined error.";

                user.Log(errorMessage);
                Log.Write(errorMessage, LogResource.Comment);
            }
        }
    }
}
