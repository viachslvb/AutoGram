using System;
using System.Collections.Generic;

namespace Database.Direct
{
    public class DirectThread
    {
        public int Id { get; set; }

        public string ThreadId { get; set; }
        public string Username { get; set; }
        public bool IsStartedDialog { get; set; }
        public bool IsFinishedDialog { get; set; }
        public long LastActivityAt { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int State { get; set; }

        public List<DirectThreadItem> Messages { get; set; }
    }
}
