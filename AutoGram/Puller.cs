using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InstagramAI.Helpers;
using Database;
using InstagramAI.Services;
using Newtonsoft.Json;

namespace AndroidDeviceController
{
    public static class Puller
    {
        private const string Path = "Users";

        public static HashSet<AndroidDevice> AndroidDevices;

        static Puller()
        {
            AndroidDevices = new HashSet<AndroidDevice>();
        }

        public static HashSet<AndroidDevice> Pull()
        {
            var files = GetFiles();

            Console.WriteLine($"All users files: {files.Count}");

            foreach (var file in files)
            {
                AndroidDevice device;

                try
                {
                    device = JsonConvert.DeserializeObject<AndroidDevice>(File.ReadAllText(file));
                }
                catch { continue; }

                if (device?.Uuid == null || device.AdvertisingId == null || device.DeviceId == null || device.DeviceString == null || device.PhoneId == null)
                {
                    continue;
                }

                if (AndroidDevices.Contains(device, new InstagramAppComparer()))
                {
                    var existedDevice = AndroidDevices.Single(x => device.Uuid == x.Uuid);
                    existedDevice.Status.Accounts++;

                    existedDevice.ScoreRisk = existedDevice.Status.Accounts;

                    Console.WriteLine($"Found double: ID #{existedDevice.Uuid} | Accounts #{existedDevice.Status.Accounts}");
                    continue;
                }

                if (device.App == null)
                {
                    var app = InstagramAppRepository.Get();
                    device.App = new InstagramApp
                    {
                        Name = app.Name,
                        Code = app.Code,
                        SignatureKey = app.SignatureKey,
                        Capabilities = app.Capabilities
                    };
                }

                device.ScoreRisk = 1;
                device.Status = new Status { Accounts = 1 };
                device.DateModified = DateTime.Now;

                AndroidDevices.Add(device);
            }

            return AndroidDevices;
        }

        private static List<string> GetFiles()
        {
            return
            (from directories in Directory.GetDirectories(Path)
             from files in Directory.GetFiles(directories)
             select files).ToList();
        }
    }
}
