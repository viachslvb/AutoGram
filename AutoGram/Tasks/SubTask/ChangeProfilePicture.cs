namespace AutoGram.Task.SubTask
{
    static class ChangeProfilePicture
    {
        public static void Do(Instagram.Instagram user, Photo picture, bool shareToFeed = false, bool loadEditProfileScreen = true)
        {
            if (loadEditProfileScreen)
                user.Do(() => user.LiveAction.ShowEditProfileScreen());

            var uploadPhotoResponse = user.Do(() => user.Internal.UploadPhotoSkipConfiguration(picture));
            var response = user.Do(() => user.Account.ChangeProfilePicture(uploadPhotoResponse.UploadId, shareToFeed));

            if (response.IsOk())
            {
                user.Log("Profile picture changed.");
                user.Storage.ProfilePhotoChanged = true;
                user.Storage.Save();
            }
            else
            {
                string errorMessage = response.IsMessage()
                    ? response.GetMessage()
                    : "Changing profile picture failed. Undefined error.";

                Log.Write(errorMessage, LogResource.ChangeProfilePicture);
                user.Log(errorMessage);
            }
        }
    }
}
