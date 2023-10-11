using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AutoGram.Storage.Model;
using Renci.SshNet;
using Renci.SshNet.Common;
using SimpleTCP;

namespace AutoGram
{
    static class ConsoleCommands
    {
        private static readonly ProcessStartInfo StartInfo = new ProcessStartInfo();

        static ConsoleCommands()
        {
            StartInfo.FileName = "cmd";
            StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StartInfo.UseShellExecute = false;
            StartInfo.RedirectStandardOutput = true;
            StartInfo.RedirectStandardError = true;
            StartInfo.RedirectStandardInput = true;
            StartInfo.CreateNoWindow = true;
        }

        public static void ResetConnection()
        {
            string[] commands = File.ReadAllLines(Variables.FileCommandResetConnection);

            foreach (var command in commands)
            {
                ExecuteCommand(command);
                Thread.Sleep(500);
            }
        }

        public static void ResetSshConnection(SshClientSettings ssh)
        {
            int errors = 0;

            while (true)
            {
                try
                {
                    using (var client = new SshClient(ssh.Host, ssh.Username, ssh.Password))
                    {
                        client.Connect();
                        IDictionary<TerminalModes, uint> termkvp = new Dictionary<TerminalModes, uint>();
                        termkvp.Add(TerminalModes.ECHO, 53);

                        ShellStream shellStream = client.CreateShellStream("xterm", 80, 24, 200, 200, 500, termkvp);
                        shellStream.WriteLine("cd /root/ && ./reconnect.sh");

                        string expectedText = shellStream.Expect(new Regex(@"Okey"), new TimeSpan(0, 0, 40));
                        client.Disconnect();

                        if (expectedText == null)
                        {
                            if (errors == 5)
                            {
                                Telegram.SendMessage(
                                    $"Server [{ssh.Host}] error: | The expected string was null [more than 5 times]", TelegramNotification.ServerRoom);
                            }

                            if (errors > 5)
                            {
                                if (Settings.Advanced.General.NotificationNightMode)
                                {
                                    for (int i = 0; i < 10; i++)
                                    {
                                        Telegram.SendMessage(
                                        $"Server [{ssh.Host}] error: | The expected string was null [more than 5 times]", TelegramNotification.ServerRoom);

                                        Thread.Sleep(1000);
                                    }
                                }
                            }

                            Thread.Sleep(5000);
                            errors++;
                            continue;
                        }

                        errors = 0;
                        break;
                    }
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Telegram.SendMessage(
                                   $"Server [{ssh.Host}] error: No response", TelegramNotification.ServerRoom);

                    if (Settings.Advanced.General.NotificationNightMode)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            Telegram.SendMessage(
                            $"Server [{ssh.Host}] error: No response", TelegramNotification.ServerRoom);

                            Thread.Sleep(1000);
                        }
                    }

                    Thread.Sleep(60000);
                }
            }
        }

        public static void ResetNetworkThroughTcpClient(TcpClientSettings tcpSettings)
        {
            var client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;

            while (true)
            {
                try
                {
                    client.Connect(tcpSettings.Host, tcpSettings.Port);
                    var message = client.WriteLineAndGetReply("mobile-network -rc", TimeSpan.FromMinutes(10));

                    if (message?.MessageString == null)
                    {
                        Telegram.SendMessage(
                            $"Tcp client [{tcpSettings.Host}:{tcpSettings.Port}] error occurred: No response received.",
                            TelegramNotification.ServerRoom);
                    }
                    else if (!message.MessageString.Contains("okay"))
                    {
                        Telegram.SendMessage(
                            $"Tcp client [{tcpSettings.Host}:{tcpSettings.Port}] error occurred: Invalid response [{message.MessageString}]",
                            TelegramNotification.ServerRoom);
                    }
                    else return;
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Telegram.SendMessage(
                        $"Tcp client [{tcpSettings.Host}:{tcpSettings.Port}] error occurred: {e.Message}",
                        TelegramNotification.ServerRoom);
                }
                catch (Exception e)
                {
                    Telegram.SendMessage(
                        $"Tcp client [{tcpSettings.Host}:{tcpSettings.Port}] error occurred: {e.Message}",
                        TelegramNotification.ServerRoom);
                }

                Thread.Sleep(25000);
            }
        }

        private static void ExecuteCommand(string command)
        {
            StartInfo.Arguments = @"/c " + command;
            Process p = Process.Start(StartInfo);
            p?.WaitForExit();
        }
    }
}
