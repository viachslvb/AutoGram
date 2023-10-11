using Database;

namespace AutoGram.Instagram.Devices
{
    class UserAgent
    {
        private const string UserAgentFormat = "Instagram {0} Android ({1}/{2}; {3}; {4}; {5}; {6}; {7}; {8}; {9}; {10})";

        public static string BuildUserAgent(Device device, InstagramApp app)
        {
            return string.Format(
                UserAgentFormat,
                app.Name,
                device.GetAndroidVersion,
                device.GetAndroidRelease,
                device.GetDpi,
                device.GetResolution,
                device.GetManufacturer,
                device.GetModel,
                device.GetDevice,
                device.GetCpu,
                device.GetUserAgentLocale,
                app.Code
            );
        }
    }
}
