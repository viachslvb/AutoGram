using System.Threading;
using AutoGram.Instagram.Response.Model;

namespace AutoGram.Task.SubTask
{
    static class Follow
    {
        public static void Do(Instagram.Instagram user, User targetUser)
        {
            if (user.LiveSettings.Follow.IsLimit)
                return;

            var followResponse = user.Do(() => user.FriendShips.Create(targetUser.Pk));

            if (followResponse.IsOk())
            {
                user.Log($"Following user {targetUser.Username}.");
                user.Activity.Actions.Follows++;

                var sleep = Utils.Random.Next(
                                    Settings.Advanced.Live.FollowSettings.Delay.From,
                                    Settings.Advanced.Live.FollowSettings.Delay.To);

                user.Log($"Sleep {sleep}s.");
                Thread.Sleep(sleep * 1000);
            }
            else
            {
                string errorMessage = followResponse.IsMessage()
                    ? followResponse.GetMessage()
                    : "Follow error.";

                user.LiveSettings.Follow.IsLimit = true;

                Log.Write(errorMessage, LogResource.Live);
                user.Log(errorMessage);
            }

            if (user.Activity.Actions.Follows >= user.LiveSettings.Follow.Limit)
            {
                user.LiveSettings.Follow.IsLimit = true;
            }
        }
    }
}
