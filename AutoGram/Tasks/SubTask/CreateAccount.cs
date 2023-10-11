using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Services;

namespace AutoGram.Task.SubTask
{
    static class CreateAccount
    {
        public static void Do(Instagram.Instagram user)
        {
            // Sign Up

            if (Settings.Advanced.Register.RegisterViaPhoneNumber.Enable)
            {
                user.Email.Username = string.Empty;
                user.SendPreLoginFlow();

                IPhoneVerificationService phoneVerificationService = Settings.Advanced.Register.RegisterViaPhoneNumber.Manual
                    ? new ManualPhoneVerification()
                    : (IPhoneVerificationService) new SmsBoostService();

                user.PhoneNumber = phoneVerificationService.GetPhoneNumber();
                user.Log($"Verification phone number: {user.PhoneNumber}");

                user.Do(() => user.Account.CheckPhoneNumber());
                Utils.RandomSleep(1000, 3000);

                var sendCodeResponse = user.Do(() => user.Account.SendSignupSmsCode());

                string errorMessage;
                if (!sendCodeResponse.IsOk())
                {
                    errorMessage = sendCodeResponse.IsMessage()
                        ? sendCodeResponse.GetMessage()
                        : "Function [sendCodeResponse] does not return [Okay]";
                    Log.Write(errorMessage);

                    throw new AccountCreateSomethingWrongException(errorMessage);
                }

                user.Log($"Sms code was sent to {user.PhoneNumber}.");

                Thread.Sleep(10000);

                string verificationCode = phoneVerificationService.ReceiveVerificationCode(user.PhoneNumber);

                var validateCodeResponse = user.Do(() => user.Account.ValidateSignupSmsCode(verificationCode));

                if (!validateCodeResponse.IsOk())
                {
                    errorMessage = sendCodeResponse.IsMessage()
                        ? sendCodeResponse.GetMessage()
                        : "Function [validateCodeResponse] does not return [Okay]";
                    Log.Write(errorMessage);

                    throw new AccountCreateSomethingWrongException(errorMessage);
                }

                user.Log($"Phone number was successfully validated.");

                //user.CreateValidated(verificationCode);
            }
            else
            {
                //user.SignUp();
            }
        }

        public static void SignupFlow(Instagram.Instagram user)
        {
            //user.SendSignupFlowAfterUploadPicture();

            if (!Settings.Advanced.Register.SkipHomeWalking)
            {
                int randomDelay = Settings.IsAdvanced
                              && Settings.Advanced.Register.ReduceDelay
                ? Utils.Random.Next(2000, 3000)
                : Utils.Random.Next(15000, 25000);

                user.Log($"Sleep {randomDelay / 1000}s");
                Thread.Sleep(randomDelay);

                user.Log("Refresh the timeline feed.");
                user.Do(() => user.LiveAction.UpdateFeedScreen());

                randomDelay = Settings.IsAdvanced
                                  && Settings.Advanced.Register.ReduceDelay
                    ? Utils.Random.Next(1000, 3000)
                    : Utils.Random.Next(8000, 20000);

                user.Log($"Sleep {randomDelay / 1000}s");
                Thread.Sleep(randomDelay);

                user.Log("Refresh the profile screen.");
                user.LiveAction.ShowProfileScreen();
            }
        }
    }
}