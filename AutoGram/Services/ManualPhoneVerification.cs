using System.Windows;
using AutoGram.Instagram.Exception;

namespace AutoGram.Services
{
    class ManualPhoneVerification : IPhoneVerificationService
    {
        public string GetPhoneNumber()
        {
            string phoneNumber = string.Empty;

            PhoneVerificationWindow phoneVerificationWindow;
            Application.Current.Dispatcher.Invoke(delegate 
            {
                phoneVerificationWindow = new PhoneVerificationWindow();

                if (phoneVerificationWindow.ShowDialog() == false)
                {
                   phoneNumber = phoneVerificationWindow.PhoneNumber;
                }
            });

            return phoneNumber;
        }

        public string ReceiveVerificationCode(string phoneNumber)
        {
            var verificationCode = string.Empty;
            var cancelRegistration = false;

            Application.Current.Dispatcher.Invoke(delegate
            {
                PhoneVerificationWindow phoneVerificationWindow = new PhoneVerificationWindow(phoneNumber);

                if (phoneVerificationWindow.ShowDialog() == false)
                {
                    verificationCode = phoneVerificationWindow.VerificationCode;
                    cancelRegistration = phoneVerificationWindow.IsCancel;
                }
            });

            if (cancelRegistration)
            {
                throw new VerificationCodeWaitingTimeoutException();
            }

            return verificationCode;
        }
    }
}
