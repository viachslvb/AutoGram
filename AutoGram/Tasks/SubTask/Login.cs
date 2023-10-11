using AutoGram.Instagram.Exception;
using Renci.SshNet.Messages;
using System.Windows;

namespace AutoGram.Task.SubTask
{
    static class Login
    {
        public static void Do(Instagram.Instagram user)
        {
            user.Log("---------------------------------------------------------------");
            user.Log($"Log In as: {user.Username}");

            var response = user.Do(() => user.Login());

            if (response.IsOk())
            {
                user.Log("Log In was successful.");
            }
            else if (response.IsInvalidCredentials())
            {
                throw new InvalidCredentialsException();
            }
            else
            {
                string errorMessage = response.IsMessage()
                    ? $"Log In failed. Error: {response.GetMessage()}"
                    : "Log In failed.";

                Log.Write(errorMessage, LogResource.Login);
                throw new SomethingWrongException(errorMessage);
            }
        }
    }
}
