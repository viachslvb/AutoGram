using System;
using System.IO;
using AutoGram.Instagram.Request;
using xNet;

namespace AutoGram.Services
{
    static class MediaTools
    {
        private static readonly object LogLocker = new object();

        public static bool MediaIsPublished(string mediaShortcode)
        {
            var request = new Request
            {
                AllowAutoRedirect = false,
                ConnectTimeout = 30000,
                ReadWriteTimeout = 30000,
            };

            int errors = 0;
            while (true)
            {
                try
                {
                    request.Get($"https://www.instagram.com/p/{mediaShortcode}/");
                    return true;
                }
                catch (HttpException exception)
                {
                    if (errors > 5)
                    {
                        LogWrite($"{exception.Message} | {exception.StackTrace}");
                        break;
                    }

                    if (exception.Status == HttpExceptionStatus.ProtocolError)
                    {
                        if (exception.HttpStatusCode == HttpStatusCode.NotFound)
                            return false;
                    }

                    errors++;
                }
            }

            return true;
        }

        public static void LogWrite(string data)
        {
            lock (LogLocker)
            {
                try
                {
                    using (TextWriter textWriter = new StreamWriter("media_tools_log.txt", true))
                    {
                        textWriter.WriteLine($"[{DateTime.Now}] {data}");
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception();
                }
            }
        }
    }
}
