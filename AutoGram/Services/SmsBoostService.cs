using System;
using System.IO;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Request;
using Newtonsoft.Json;
using xNet;

namespace AutoGram.Services
{
    class SmsBoostService : IPhoneVerificationService
    {
        private const string ApiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlYWU0MjcxNC03M2I0LTQ2MmItOGE1Yy00MzRlZTc4NzUxMjkiLCJqdGkiOiJkNjliODVlZC00ODdiLTRhZTctYWJiZi00ZGMyZDllZmQ2OTciLCJpYXQiOiIxNTYyMzk3NTI0IiwibmJmIjoxNTYyMzk3NTI0LCJleHAiOjE1Nzc5NDk1MjQsImlzcyI6InNtc2Jvb3N0LnJ1IiwiYXVkIjoiQ3VzdG9tZXJzIn0.x3PjN0rq75Ar6Sq0_CPdOaq0eksn-j3e0BueUMHe23g";
        private static readonly object LogLocker = new object();
        private static int _errorCounter;

        private readonly Request _request;

        public SmsBoostService()
        {
            _request = new Request
            {
                BaseAddress = new Uri("http://smsboost.ru/"),
                Authorization = $"Bearer {ApiToken}",
                AllowAutoRedirect = false,
                //IgnoreProtocolErrors = true,
                ConnectTimeout = 30000,
                ReadWriteTimeout = 30000,
            };

#if DEBUG
            _request.Proxy = HttpProxyClient.Parse($"127.0.0.1:8082");
            _request.Proxy.ConnectTimeout = 40000;
            _request.Proxy.ReadWriteTimeout = 40000;
#endif

            if (_errorCounter >= 30)
                throw new VerificationServiceErrorLimitException();
        }

        public string GetPhoneNumber()
        {
            while (true)
            {
                try
                {
                    _request.SetCustomRequest();
                    _request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    _request.AddHeader("Host", "smsboost.ru");

                    var response = _request.Get("api/v1/platform/phone/instagram")
                        .ToResponse<SmsBoostServiceResponse>();

                    return response.Message;
                }
                catch (HttpException exception)
                {
                    if (exception.Status == HttpExceptionStatus.ProtocolError)
                    {
                        if (exception.HttpStatusCode == HttpStatusCode.ImATeapot)
                            throw new VerificationServiceZeroBalanceException();

                        LogWrite($"{exception.HttpStatusCode}");
                        _errorCounter++;

                        throw new VerificationServiceFailedException();
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        public string ReceiveVerificationCode(string phoneNumber)
        {
            var timeOnStarting = Utils.DateTimeNowTotalSeconds;

            while (true)
            {
                try
                {
                    _request.SetCustomRequest();
                    _request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    _request.AddHeader("Host", "smsboost.ru");

                    var response = _request.Get($"api/v1/platform/code/{phoneNumber}")
                        .ToResponse<SmsBoostServiceResponse>();

                    return response.Message;
                }
                catch (HttpException exception)
                {
                    if (exception.Status == HttpExceptionStatus.ProtocolError)
                    {
                        switch (exception.HttpStatusCode)
                        {
                            case HttpStatusCode.ImATeapot:
                                throw new VerificationCodeWaitingTimeoutException();
                            case HttpStatusCode.NotFound:
                            case HttpStatusCode.Found:
                                if (Utils.DateTimeNowTotalSeconds - timeOnStarting > 180)
                                {
                                    CancelVerification(phoneNumber);
                                    throw new VerificationCodeWaitingTimeoutException();
                                }

                                Thread.Sleep(10000);

                                continue;
                        }

                        LogWrite($"{exception.HttpStatusCode}");
                        _errorCounter++;

                        throw new VerificationServiceFailedException();
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        private void CancelVerification(string phoneNumber)
        {
            while (true)
            {
                try
                {
                    _request.SetCustomRequest();
                    _request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    _request.AddHeader("Host", "smsboost.ru");

                    _request.Get($"api/v1/platform/cancel/{phoneNumber}");
                    break;
                }
                catch (HttpException exception)
                {
                    if (exception.Status == HttpExceptionStatus.ProtocolError)
                    {
                        LogWrite($"{exception.HttpStatusCode} | In cancelVerification");
                        break;
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        public static void LogWrite(string data)
        {
            lock (LogLocker)
            {
                try
                {
                    using (TextWriter textWriter = new StreamWriter(Variables.FileVerificationServiceLog, true))
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

    class SmsBoostServiceResponse
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
