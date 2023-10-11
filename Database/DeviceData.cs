using System;

namespace Database
{
    public class DeviceData
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string PhoneId { get; set; }
        public string DeviceId { get; set; }
        public string UserAgent { get; set; }
        public int Used { get; set; }
        public int Accounts { get; set; }
        public DateTime DateModified { get; set; }
    }
}
