using System;
using System.ComponentModel.DataAnnotations;

namespace Database
{
    public class AndroidDevice
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string DeviceId { get; set; }
        public string PhoneId { get; set; }
        public string AdvertisingId { get; set; }
        public string DeviceString { get; set; }

        [Required]
        public InstagramApp App { get; set; }

        [Required]
        public Status Status { get; set; }

        public int ScoreRisk { get; set; }
        public DateTime DateModified { get; set; }
    }
}