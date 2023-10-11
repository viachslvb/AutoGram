using System.IO;
using System.Windows;
using Database;
using AutoGram.Instagram.Exception;
using Newtonsoft.Json;

namespace AutoGram.Instagram.Devices
{
    class Device
    {
        private string _androidVersion;
        private string _androidRelease;
        private string _dpi;
        private string _resolution;
        private string _manufacturer;
        private string _model;
        private string _device;
        private string _cpu;
        private string _userAgent;
        private string _deviceString;
        private string _userAgentLocale;

        private static readonly DeviceData DeviceData;

        static Device()
        {
            DeviceData = JsonConvert.DeserializeObject<DeviceData>(File.ReadAllText("devices.json"));

            DeviceData.Devices.Shuffle();
            DeviceData.AndroidVersions.Shuffle();
            DeviceData.PixelsPerInch.Shuffle();
            DeviceData.Resolutions.Shuffle();
        }

        public Device(InstagramApp app)
        {
            string deviceString = DeviceData.Devices[Utils.Random.Next(DeviceData.Devices.Length)];

            InitFromDeviceString(deviceString, app);
        }

        public Device(string deviceString, InstagramApp app)
        {
            InitFromDeviceString(deviceString, app, true);
        }

        public string GetAndroidVersion => _androidVersion;

        public string GetAndroidRelease => _androidRelease;

        public string GetDpi => _dpi;

        public string GetResolution => _resolution;

        public string GetManufacturer => _manufacturer;

        public string GetModel => _model;

        public string GetDevice => _device;

        public string GetCpu => _cpu;

        public string GetUserAgent => _userAgent;

        public string GetDeviceString => _deviceString;

        public string GetUserAgentLocale => _userAgentLocale;

        private void InitFromDeviceString(string deviceString, InstagramApp app, bool exportData = false)
        {
            bool randomize = false;

            try
            {
                string[] parts = deviceString.Split(';');

                string[] androidOs = randomize ?
                    DeviceData.AndroidVersions[Utils.Random.Next(DeviceData.AndroidVersions.Length)].Split('/')
                    : parts[0].Split('/');
                
                this._androidVersion = androidOs[0];
                this._androidRelease = androidOs[1];
                
                this._dpi = randomize ?
                    DeviceData.PixelsPerInch[Utils.Random.Next(DeviceData.PixelsPerInch.Length)]
                    : parts[1];
                
                this._resolution = randomize ?
                    DeviceData.Resolutions[Utils.Random.Next(DeviceData.Resolutions.Length)]
                    : parts[2];
                
                this._manufacturer = parts[3];
                this._model = parts[4];
                this._device = parts[5];
                this._cpu = parts[6];
                
                this._userAgentLocale = parts.Length > 7 && !string.IsNullOrEmpty(parts[7])
                    ? parts[7]
                    : Constants.UserAgentLocale;
            }
            catch (System.Exception)
            {
                throw new DeviceFormatInvalidException();
            }

            this._userAgent = UserAgent.BuildUserAgent(this, app);
            this._deviceString = DeviceToString(this);
        }

        private string DeviceToString(Device device)
        {
            return $"{device.GetAndroidVersion}/{device.GetAndroidRelease};{device.GetDpi};{device.GetResolution};" +
                   $"{device.GetManufacturer};{device.GetModel};{device.GetDevice};{device.GetCpu};";
        }
    }
}
