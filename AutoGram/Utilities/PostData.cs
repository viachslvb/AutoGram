using System.IO;

namespace AutoGram
{
    class PostData : MediaObject
    {
        private static readonly object Lock = new object();

        public PostData(string picturePath, string caption, bool isProfileUrl = false) : base(picturePath, isProfileUrl)
        {
            lock (Lock)
            {
                this.Image = File.ReadAllBytes(System.IO.Path.Combine(Variables.FolderPosts, picturePath));
            }

            UpdateSize();

            if (Settings.Advanced.Post.Content.AddComment)
            {
                this.Caption = "";
                this.Comment = caption;

            }
            else
            {
                this.Caption = caption;
            }
        }
    }
}
