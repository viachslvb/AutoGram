using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DirectSender
{
    public class UserDirect
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Pk { get; set; }
        public string FullName { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsProcessed { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
