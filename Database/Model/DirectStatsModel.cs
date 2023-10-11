using System;
using System.Collections.Generic;
using System.Linq;
namespace Database.Model
{
    public class DirectStatsModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string From { get; set; }
        public string Recipient { get; set; }
        public string SourcePk { get; set; }
        public string SourceUsername { get; set; }
    }
}