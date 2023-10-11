using System;
using System.Threading;
using System.Windows;
using Database;
using AutoGram.Instagram.Exception;
using AutoGram.Services;
using xNet;

namespace AutoGram.Task
{
    static class Register
    {
        private static readonly object LockerSavingInFile = new object();
        private static readonly string FileCreatedAccounts;
        private static readonly object UpdateStatsLock = new object();

        static Register()
        {
            FileCreatedAccounts = Variables.FolderRegisterModule + "/" + Variables.FileCreatedAccounts;
        }

        public static Instagram.Instagram Do(Worker worker)
        {
            RandomUserData randomUserData = RandomUserData.Get();

            worker.Account.WriteLog("---------------------------------------------------------------");
            worker.Account.WriteLog($"Sign up: {randomUserData.UserName}");

            AndroidDevice androidDevice = null;
            DeviceData deviceData = null;

            #region Android Device Settings
            if (Settings.Advanced.Register.Device.UseAndroidDeviceDatabase)
            {
                using (var repo = new AndroidDeviceDisconnectedRepository())
                {
                    androidDevice = repo.Get();
                }
            }

            if (Settings.Advanced.Register.Device.UseDeviceDataDatabase)
            {
                if (!DeviceDataRepository.Any())
                    throw new SomethingWrongException("Device data repository is empty.");

                deviceData = DeviceDataRepository.GetDevicaData();
                deviceData.Used++;
                DeviceDataRepository.Update(deviceData);

                androidDevice = DeviceDataToAndroidDevice(deviceData);
            }

            if (Settings.Advanced.Register.Device.UseDeviceDataFile)
            {
                androidDevice = PhoneRepository.Get();
            }
            #endregion

            var user = new Instagram.Instagram(randomUserData.UserName, randomUserData.Password, randomUserData.Email, randomUserData, androidDevice, worker: worker);

            if (worker.IsSettings)
            {
                if (worker.Settings.UseLocalIp)
                {
                    if (user.IsProxy())
                        user.RemoveProxy();
                }

                if (worker.Settings.UseCustomProxy)
                {
                    user.SetProxy(worker.Settings.Proxy);
                }

                if (worker.Settings.UseProxyFromFile)
                {
                    if (!user.IsProxy())
                        Proxy.Set(user);
                }
            }
            else
            {
                if (!user.IsProxy())
                    Proxy.Set(user);
            }

            try
            {
                SubTask.CreateAccount.Do(user);
                SaveAccountToFile(user, FileCreatedAccounts);

                user.Log($"We are successfully was created account {user.Username}");

                user.Activity = user.Storage.Activity;
                user.Storage.Save();

                #region Save Results

                if (Settings.Advanced.Register.Device.UseAndroidDeviceDatabase)
                {
                    androidDevice.Status.Success++;
                    androidDevice.Status.Accounts++;

                    using (var repo = new AndroidDeviceDisconnectedRepository())
                    {
                        repo.Update(androidDevice);
                    }
                }

                if (Settings.Advanced.Register.Device.UseDeviceDataDatabase)
                {
                    if (deviceData != null)
                    {
                        deviceData.Accounts++;
                        DeviceDataRepository.Update(deviceData);
                    }
                }

                lock (UpdateStatsLock)
                    Worker.RegisteredSuccess++;

                #endregion

                worker.LocalRegistrationErrorsChain = 0;

                #region Upload profile picture

                if (Settings.Advanced.Register.UploadProfilePicture)
                {
                    if (Settings.Basic.Register.RandomUploadProfilePicture && Utils.UseIt() ||
                        !Settings.Basic.Register.RandomUploadProfilePicture)
                    {
                        Utils.RandomSleep(3000, 6000);
                        user.Log($"Uploading profile picture...");

                        var profilePhoto = Photos.GetProfilePhoto(worker.Folder);
                        var picture = new Photo(profilePhoto, false, true);

                        SubTask.ChangeProfilePicture.Do(user, picture,
                            Settings.Basic.Register.ShareToFeedProfilePicture, loadEditProfileScreen: false);
                    }
                }

                #endregion

                if (!Settings.Advanced.Register.SkipSignupFlow)
                {
                    SubTask.CreateAccount.SignupFlow(user);
                }

                return user;
            }
            catch (ChallengeRequiredException)
            {
                user.Log("Challenge required. Captcha.");

                lock (UpdateStatsLock)
                    Worker.RegisteredFailed++;

                worker.LocalRegistrationErrorsChain++;
                worker.LocalRegistrationFailed++;
            }
            catch (AccountCreateFailedException)
            {
                user.Log("Signup failed. Feedback required.");

                #region Save Results

                if (Settings.Advanced.Register.Device.UseAndroidDeviceDatabase)
                {
                    androidDevice.Status.Failure++;

                    using (var repo = new AndroidDeviceDisconnectedRepository())
                    {
                        repo.Update(androidDevice);
                    }
                }

                lock (UpdateStatsLock)
                    Worker.RegisteredFailed++;

                worker.LocalRegistrationErrorsChain++;
                worker.LocalRegistrationFailed++;

                #endregion
            }
            catch (EmailAlreadyUsedException)
            {
                user.Log("Email already used. Next.");
            }
            catch (AccountCreateSomethingWrongException exception)
            {
                user.Log(exception.Message);
            }
            catch (VerificationCodeWaitingTimeoutException)
            {
                user.Log("Timeout waiting for verification code.");
            }
            catch (VerificationServiceErrorLimitException)
            {
                user.Log("Verification service error limit exception.");
                throw new SuspendAccountCreatingException();
            }
            catch (VerificationServiceZeroBalanceException)
            {
                user.Log("Verification service zero balance exception.");
                throw new SuspendAccountCreatingException();
            }
            catch (VerificationServiceFailedException)
            {
                user.Log("Verification service failed exception.");
                throw new AccountCreateFailedException();
            }
            finally
            {
                lock (UpdateStatsLock)
                {
                    Worker.UpdateRegistrationStats();

                    if (Worker.RegisteredSuccess > Settings.Advanced.Register.Limits.Accounts)
                    {
                        throw new AccountCreateLimitException();
                    }

                    if (Worker.RegisteredFailed >= Settings.Advanced.Register.Limits.GlobalFailed)
                    {
                        throw new AccountCreateFailedLimitException();
                    }

                    if (worker.LocalRegistrationErrorsChain >= Settings.Advanced.Register.Limits.MaximumErrorsChain)
                    {
                        throw new AccountCreateFailedLimitException();
                    }

                    if (worker.LocalRegistrationFailed >= Settings.Advanced.Register.Limits.LocalFailed)
                    {
                        throw new AccountCreateFailedLimitException();
                    }
                }
            }

            throw new AccountCreateFailedException();
        }

        private static void SaveAccountToFile(Instagram.Instagram account, string filePath)
        {
            string accountString = $"{account.Username}:{account.Password}:";

            accountString += !string.IsNullOrEmpty(account.Email.Username)
                ? $"{account.Email.Username}:{account.Email.Password}"
                : $"Phone:{account.PhoneNumber}";

            lock (LockerSavingInFile)
            {
                Utils.WriteToFile(filePath, accountString);
            }
        }

        private static AndroidDevice DeviceDataToAndroidDevice(DeviceData deviceData)
        {
            var deviceString = Utils.TryParse(deviceData.UserAgent, @"(?<=Android.\()(.+)");
            deviceString = deviceString.Replace("; ", ";");

            var androidDevice = new AndroidDevice
            {
                AdvertisingId = Utils.GenerateUUID(true),
                App = InstagramAppRepository.Get(),
                DeviceId = deviceData.DeviceId,
                PhoneId = deviceData.PhoneId,
                DeviceString = deviceString,
                Uuid = deviceData.Uuid
            };

            return androidDevice;
        }
    }
}