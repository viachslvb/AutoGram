namespace AutoGram.Services
{
    interface IPhoneVerificationService
    {
        string GetPhoneNumber();
        string ReceiveVerificationCode(string phoneNumber);
    }
}
