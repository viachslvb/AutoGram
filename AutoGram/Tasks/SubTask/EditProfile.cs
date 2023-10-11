using AutoGram.Instagram.Response;
using System.Linq;
using System.Windows;

namespace AutoGram.Task.SubTask
{
    static class EditProfile
    {
        public static void Do(Instagram.Instagram user, string fullName = null)
        {
            var newUserData = RandomUserData.Get();

            UserResponse userResponse = new UserResponse();

            // Real flow api requests

            user.State.IgNavChain = "1nj:feed_timeline:1,8BQ:self_profile:2";

            user.Do(() => user.Feed.GetUserFeedStory());
            user.Do(() => user.Feed.GetUserFeed());
            user.Do(() => user.Internal.HighLightsTray());
            user.Do(() => user.Account.GetUserInfo(entryPoint: "self_profile", fromModule: "self_profile"));
            user.Do(() => user.Internal.ProfileArchiveBadge());
            user.Do(() => user.Discover.GetSuggestedUsers(module: "self_profile"));
            
            // Get current user info

            userResponse = user.Do(() => user.Account.GetCurrentUser(edit: true));

            user.State.IgNavChain = "1nj:feed_timeline:1,8BQ:self_profile:2,73j:edit_profile:3";
            
            user.Do(() => user.Internal.CanCreatePersonalFundraisers());
            user.Do(() => user.Internal.SetContactPointPrefill());

            // Check an username availability if we need to change him

            if (Settings.Advanced.Profile.ChangeUsernameToDefault)
            {
                user.Do(() => user.Account.CheckUsername(newUserData.UserName));
            }

            // Phone number

            if (userResponse.User?.Phone_number != string.Empty)
            {
                //userResponse.User.Phone_number = string.Empty;
                userResponse.User.Email = newUserData.Email.Username;
            }

            // Full name

            userResponse.User.Full_name = $"{newUserData.FirstName} {newUserData.LastName}";

            // Profile username changing

            if (Settings.Advanced.Profile.ChangeUsernameToDefault)
            {
                userResponse.User.Username = newUserData.UserName;

                user.Worker.Account.SetName(newUserData.UserName);
                user.Worker.Account.SetLink(newUserData.UserName);
                user.Username = userResponse.User.Username;
                Utils.WriteToFile("changed_accounts.txt", $"{newUserData.UserName}:{user.Password}:{newUserData.UserName}@gmail.com:{user.Password}");
                user.Storage.RenameStorage(user.Username);
            }

            // Profile Description
            var profileDescList = Settings.Advanced.Profile.ProfileDescriptions.ToList();
            var profileDesc = userResponse.User.Biography;

            if (Settings.Advanced.Profile.ChangeBiography)
            {
                profileDesc = profileDescList[Utils.Random.Next(0, profileDescList.Count)];
                var biographyResp = user.Do(() => user.Account.SetBiography(profileDesc));

                if (biographyResp.IsOk())
                {
                    user.Log("Biography edited.");
                    userResponse.User.Biography = profileDesc;
                    user.Storage.ProfileEdited = true;
                    user.Storage.Save();

                    user.Do(() => user.Internal.CanCreatePersonalFundraisers());
                    user.Do(() => user.Internal.SetContactPointPrefill());
                }
            }

            userResponse = user.Do(() => user.Account.EditProfile(userResponse.User));

            if (userResponse.IsOk())
            {
                user.Log("Profile edited.");
                user.UserInfo = userResponse;
                user.Storage.ProfileEdited = true;
                user.Storage.Save();
            }
            else
            {
                if (userResponse.IsMessage())
                {
                    user.Log(userResponse.GetMessage());
                }
            }

            if (Settings.Advanced.Profile.IsPrivateProfile)
            {
                if (!userResponse.User.IsPrivate)
                {
                    user.Do(() => user.Account.SetPrivate());
                }
            }

            user.Do(() => user.Internal.FundRaiser());
        }
    }
}
