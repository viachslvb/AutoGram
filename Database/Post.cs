using System;

namespace Database
{
    public class Post
    {
        public int Id { get; set; }
        public int Likes { get; set; }
        public string Caption { get; set; }
        public string PictureUrl { get; set; }
        public string PictureLocalPath { get; set; }
        public string Tags { get; set; }
        public bool Consumed { get; set; }
        public int Used { get; set; }
        public DateTime DateModified { get; set; }
    }
}
