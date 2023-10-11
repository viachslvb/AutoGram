using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoGram.Instagram.Exception;
using AutoGram.Task.SubTask;
using Renci.SshNet.Messages;

namespace AutoGram.Task
{
    static class CreateChildAccounts
    {
        private static readonly object LockerSavingInFile = new object();
        private static readonly string FileCreatedAccounts;
        private static readonly object UpdateStatsLock = new object();

        static CreateChildAccounts()
        {
            FileCreatedAccounts = Variables.FolderRegisterModule + "/" + "child_created_accounts.txt";
        }

        public static void Do(Instagram.Instagram user)
        {
            // Save current user storage
            user.SaveStorage();
            user.DisableStorageSaving = true;

            // Get main user session token
            string mainUserSessionToken = user.GetCookieValue("sessionid");
            string mainUsername = user.Username;

            // Clear unnecessary cookies
            var acceptableCookies = new List<string>() { "mid", "csrftoken", "rur" };
            var cookiesToDelete = user.GetCookies().Where(x => !acceptableCookies.Contains(x.Key)).ToArray();

            foreach (var cookie in cookiesToDelete)
                user.RemoveCookieByKey(cookie.Key);

            // Auth state to null
            user.State.Authorization = null;
            user.State.IgWwwClaim = null;

            // Creating child account
            user.Log("Creating child account...");

            var randomUserData = RandomUserData.Get(true);

            // Pre creating flow
            user.Do(() => user.Internal.ReadMsisdnHeader(useCsrfToken: true));
            user.Do(() => user.Internal.SetContactPointPrefill());
            user.Do(() => user.Internal.SyncDeviceFeatures(preLogin: true, useCsrfToken: true));
            user.Do(() => user.Internal.PreLoginLauncherSync(useCsrfToken: true));

            Utils.RandomSleep(4000, 8000);

            // Check username suggestions 
            var username = randomUserData.UserName.ToLower();
            string suggestedUsername = $"{randomUserData.UserName}{Utils.GetRandomNumber(5, 7)}".ToLower();

            while (true)
            {
                var suggestionResponse = user.Do(() => user.Account.CheckUsername(username));
                Utils.RandomSleep(2000, 4000);

                if (suggestionResponse.IsAvailable)
                {
                    user.Username = username;
                    break;
                }

                if (username == suggestedUsername)
                {
                    suggestedUsername = $"{suggestedUsername}{Utils.GetRandomNumber(1, 2)}";
                }
                else
                {
                    username = suggestionResponse.GetSuggestions() != null
                        ? suggestionResponse.GetSuggestions()[
                            Utils.Random.Next(0, suggestionResponse.GetSuggestions().Count)]
                        : suggestedUsername;
                }
            }

            //user.Log($"Available username: {user.Username}");
            Utils.RandomSleep(1000, 2000);

            var configResponse = user.Do(() => user.Internal.ConsentGetSignupConfig());

            // Birthday
            var day = Utils.CreateRandomNumb(1, 26);
            var month = Utils.CreateRandomNumb(1, 12);
            var year = Utils.CreateRandomNumb(1988, 2001);

            if (configResponse.IsAgeRequired)
            {
                //user.Log($"Set user age: {day}.{month}.{year}.");
                Utils.RandomSleep(4000, 8000);

                user.Do(() => user.Internal.ConsentCheckAgeEligibility(day, month, year));
                Utils.RandomSleep(1000, 2000);
            }

            string fullname = $"{randomUserData.FirstName} {randomUserData.LastName}";
            bool fillFullnameDirectly = Settings.Advanced.CreatorChildAccounts.FillFullnameDirectly;

            var accountCreateResponse = user.Do(() => user.Account.CreateSecondaryAccount(mainUserSessionToken,
                user.AccountId, configResponse.TosVersion, day, month, year, fullname, fillFullnameDirectly));

            // Creation account
            bool isSuccessfully = false;

            if (accountCreateResponse.IsCreated)
            {
                user.Log($"Account {user.Username} was created successfully.");
                isSuccessfully = true;

                lock (UpdateStatsLock)
                    Worker.RegisteredSuccess++;

                user.AccountId = accountCreateResponse.User.Pk;
                user.RankToken = $"{user.AccountId}_{user.Uuid}";

                SaveAccountToFile(user);

                // Post registration flow

                if (Settings.Advanced.CreatorChildAccounts.UsePostRegistrationFlow)
                {
                    user.Do(() => user.Account.GetAccountFamily());
                    user.Do(() => user.Internal.PostLoginLauncherSync());
                    user.Do(() => user.Internal.SetContactPointPrefill(usage: "auto_confirmation", isCreatedAccountUsage: true));
                    user.Do(() => user.Internal.FetchZeroRatingToken());
                    user.Do(() => user.Internal.NewAccountNuxSeen());
                    user.Do(() => user.Internal.DynamicOnboardingGetSteps(true, true));
                    user.Do(() => user.Internal.SyncUserFeatures());
                    user.Do(() => user.Internal.AddressBookLink());

                    bool uploadedProfilePicture = false;
                    if (Settings.Advanced.CreatorChildAccounts.UploadProfilePicture)
                    {
                        Utils.RandomSleep(3000, 6000);
                        user.Log($"Uploading profile picture...");

                        var profilePhoto = Photos.GetProfilePhoto(user.Worker.Folder);
                        var picture = new Photo(profilePhoto, false, true);

                        SubTask.ChangeProfilePicture.Do(user, picture, Settings.Advanced.CreatorChildAccounts.ShareToFeedProfilePicture, loadEditProfileScreen: false);
                        uploadedProfilePicture = true;
                    }

                    user.Do(() => user.Internal.DynamicOnboardingGetSteps(emptySeenSteps: false, isSecondaryAccountCreation: true, progressState: "finish"));

                    user.Log("Registration flow...");
                    //user.SendSignupFlowAfterUploadPicture();

                    // Set private profile
                    bool profileScreenIsOpen = false;

                    if (Settings.Advanced.CreatorChildAccounts.SetPrivateProfile)
                    {
                        user.Log("Refresh profile screen.");
                        user.Do(() => user.LiveAction.ShowProfileScreen());
                        Utils.RandomSleep(2000, 3000);

                        user.Log("Open settings and set private profile.");
                        user.Do(() => user.LiveAction.ShowSettingsScreen());
                        Utils.RandomSleep(2000, 3000);
                        user.Do(() => user.Account.SetPrivate());

                        profileScreenIsOpen = true;
                    }

                    if (Settings.Advanced.CreatorChildAccounts.EditProfile)
                    {
                        if (!profileScreenIsOpen)
                        {
                            user.Log("Refresh profile screen.");
                            user.Do(() => user.LiveAction.ShowProfileScreen());
                        }

                        Utils.RandomSleep(2000, 3000);
                        user.Log("Edit profile.");
                        EditProfile.Do(user, $"{randomUserData.FirstName} {randomUserData.LastName}");
                    }
                }
                else if (Settings.Advanced.CreatorChildAccounts.UseLightVersionForEditingProfile)
                {
                    if (Settings.Advanced.CreatorChildAccounts.UploadProfilePicture)
                    {
                        user.Log($"Uploading profile picture...");

                        var profilePhoto = Photos.GetProfilePhoto(user.Worker.Folder);
                        var picture = new Photo(profilePhoto, false, true);

                        SubTask.ChangeProfilePicture.Do(user, picture, Settings.Advanced.CreatorChildAccounts.ShareToFeedProfilePicture, loadEditProfileScreen: false);
                    }

                    Utils.RandomSleep(1000, 2000);

                    if (Settings.Advanced.CreatorChildAccounts.SetPrivateProfile)
                    {
                        user.Do(() => user.Account.SetPrivate());
                    }

                    if (Settings.Advanced.CreatorChildAccounts.EditProfile)
                    {
                        user.Log("Edit profile.");
                        EditProfile.Do(user, $"{randomUserData.FirstName} {randomUserData.LastName}");
                    }
                }

                user.Storage = new Instagram.Settings.Storage(user);
                user.Storage.Save();
            }
            else
            {
                user.Log(accountCreateResponse.IsSpam() ? "Feedback required." : "Something wrong.");

                lock (UpdateStatsLock)
                    Worker.RegisteredFailed++;

                user.Worker.LocalRegistrationErrorsChain++;
                user.Worker.LocalRegistrationFailed++;

                user.Username = mainUsername;
                user.IsAccountCreatedChallenge = true;
                user.SaveStorage();
            }

            lock (UpdateStatsLock)
            {
                Worker.UpdateRegistrationStats();

                if (user.Worker.LocalRegistrationErrorsChain >= Settings.Advanced.CreatorChildAccounts.FailsLimit)
                {
                    throw new AccountCreateFailedLimitException();
                }
            }

            if (!isSuccessfully)
                throw new AccountCreateFailedException();
        }

        private static void SaveAccountToFile(Instagram.Instagram account)
        {
            string cookieString = string.Empty;

            foreach (var cookie in account.GetCookies())
            {
                cookieString += $"{cookie.Key}={cookie.Value};";
            }

            string accountString = $"{account.Username}:{account.Password}|{account.GetUserAgent()}|{account.DeviceId};" +
                                   $"{account.PhoneId};{account.Uuid};{account.AdvertisingId}|{cookieString}||";

            lock (LockerSavingInFile)
            {
                Utils.WriteToFile(FileCreatedAccounts, accountString);
            }
        }
    }
}
