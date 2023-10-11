using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Direct
{
    public class DirectThreadItem
    {
        public int Id { get; set; }

        public string ItemId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public long Timestamp { get; set; }

        public int ThreadForeignKey { get; set; }
        public DirectThread Thread { get; set; }
    }
}
