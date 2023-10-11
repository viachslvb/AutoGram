using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram
{
    class Video : MediaObject
    {
        private static readonly object Lock = new object();

        public Video(string path, bool isProfileUrl = false) : base(path, isProfileUrl)
        {
            Width = 640;
            Height = 1138;

            lock (Lock)
            {
                this.Video = File.ReadAllBytes(path);
                this.Image =
                    File.ReadAllBytes("bin/videoPreview.jpg");
            }

            UpdateCaption();
        }
    }
}
