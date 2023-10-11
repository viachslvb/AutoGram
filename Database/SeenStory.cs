using System;

namespace Database
{
    public class SeenStory
    {
        public int Id { get; set; }
        public string OwnerPk { get; set; }
        public string OwnerUsername { get; set; }
        public string OwnerFullname { get; set; }
        public string StoryPk { get; set; }
        public int SourceType { get; set; }
        public string Source { get; set; }
        public DateTime DateModified { get; set; }
    }
}
