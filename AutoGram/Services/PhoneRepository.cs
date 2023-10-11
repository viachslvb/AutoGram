using System;
using System.Collections.Generic;
using System.IO;
using Database;

namespace AutoGram.Services
{
    public static class PhoneRepository
    {
        public static List<AndroidDevice> Phones;
        private static int _counter;

        private static readonly object _lock = new object();

        static PhoneRepository()
        {
            Phones = new List<AndroidDevice>();

            var phones = File.ReadAllLines(Variables.FilePhones);

            foreach (var phone in phones)
            {
                var phoneId = phone.Split(',')[0];
                var deviceId = phone.Split(',')[1];
                var uuid = phone.Split(',')[2];
                var useragent = phone.Split(',')[3];

                var deviceString = Utils.TryParse(useragent, @"(?<=Android.\()(.+)");
                deviceString = deviceString.Replace("; ", ";");

                var androidDevice = new AndroidDevice
                {
                    AdvertisingId = Utils.GenerateUUID(true),
                    App = InstagramAppRepository.Get(),
                    DeviceId = deviceId,
                    PhoneId = phoneId,
                    DeviceString = deviceString,
                    Uuid = uuid
                };

                Phones.Add(androidDevice);
            }
        }

        public static AndroidDevice Get()
        {
            lock (_lock)
            {
                if (Phones.Count > _counter)
                    return Phones[_counter++];

                _counter = 0;
                return Phones[_counter++];
            }
        }
    }
}