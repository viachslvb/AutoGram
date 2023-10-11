using AutoGram.Instagram.Response.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Task.SubTask
{
    static class FollowUsersList
    {
        private static readonly List<string> UsersList;

        static FollowUsersList()
        {
            string users = Settings.Advanced.Live.FollowUsersOnStarting.Users;
            UsersList = users.Replace(" ", "").Split(',').ToList();
        }

        public static void Do(Instagram.Instagram user)
        {
            if (user.Storage.IsFollowedUsersOnStarting) return;

            user.Log("Following the users list.");

            foreach (var userPk in UsersList)
            {
                var userTarget = new User { Pk = userPk };

                var userResponse = user.Do(() => Profile.Open(user, userTarget));
                user.Log($"Open profile {userResponse.UserInfo.User.Username}");

                Follow.Do(user, userResponse.UserInfo.User);

                if (user.LiveSettings.Follow.IsLimit)
                {
                    user.Log($"Followings are limited.");
                    break;
                }

                user.Storage.IsFollowedUsersOnStarting = true;
                user.Log("Sleep 6 s.");
            }
        }
    }
}
