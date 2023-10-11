using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using AForge.Imaging.Filters;
using AForge.Video.FFMPEG;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using AutoGram.ImageUnique;

namespace AutoGram
{
    class Photo : MediaObject
    {
        private static readonly Object Lock = new object();
        private static readonly Random Random = new Random();
        private static readonly Semaphore Semaphore = new Semaphore(2, 2);

        public Photo(string path, bool makeVideo = false, bool isProfilePhoto = false, bool isProfileUrl = false, bool isDirectPhoto = false) : base(path, isProfileUrl)
        {
            byte[] photoBytes;

            if (!Settings.Advanced.Image.EnableImaginaryPlus)
            {
                photoBytes = File.ReadAllBytes(path);
            }
            else
            {
                lock (Lock)
                {
                    photoBytes = new ImaginaryPlus(File.ReadAllBytes(path), isProfilePhoto, isDirectPhoto).ToBytes();
                }
            }

            this.Image = photoBytes;

            UpdateSize();

            if (makeVideo)
                CreateMotion();

            Update();
        }

        public void CreateMotion()
        {
            if (Settings.Advanced.Post.Type.UseVideoKik)
            {
                lock (Lock)
                {
                    this.Video = File.ReadAllBytes($"somedata");
                    return;
                }
            }

            // Video Settings
            var frameRate = Variables.IsSharedVersion ? 30 : 20;
            var frameLoop = Variables.IsSharedVersion ? 100 : 40;

            // Fast Settings
            if (Variables.IsFastVersion && !Variables.IsSharedVersion)
            {
                frameRate = 20;
                frameLoop = 30;
            }

            var currentDirectory = Directory.GetParent(this.Path);

            if (File.Exists($"{currentDirectory}/video.mp4"))
                File.Delete($"{currentDirectory}/video.mp4");

            if (File.Exists($"{currentDirectory}/video.avi"))
                File.Delete($"{currentDirectory}/video.avi");

            if (!Variables.IsSharedVersion)
                Semaphore.WaitOne();

            using (VideoFileWriter writer = new VideoFileWriter())
            {
                var width = Width % 2 != 0 ? Width + 1 : Width;
                var height = Height % 2 != 0 ? Height + 1 : Height;

                writer.Open($"{currentDirectory}/video.avi", width, height, frameRate, VideoCodec.MPEG4);

                bool goBack = false;

                for (var i = 0; i > -1;)
                {
                    if (i < frameLoop && !goBack) i++;
                    else
                    {
                        goBack = true;
                        i--;
                    }

                    using (MemoryStream inStream = new MemoryStream(this.Image))
                    {
                        using (Bitmap bmp = new Bitmap(inStream))
                        {
                            Bitmap frame = Resize(bmp, width, height);

                            if (!Variables.IsSharedVersion && Settings.Basic.Image.MakeVideoMotion)
                            {
                                Crop filter = new Crop(new Rectangle(0, 0, width - i, height - i));
                                frame = filter.Apply(frame);
                                frame = Resize(frame, width, height);
                            }

                            writer.WriteVideoFrame(frame);
                            frame.Dispose();

                            Thread.Sleep(250);
                        }
                    }

                    if (!Variables.IsSharedVersion)
                    {
                        GC.Collect();
                    }

                    if (Variables.IsSharedVersion && i % 10 == 0)
                    {
                        GC.Collect();
                    }
                }
            }

            if (!Variables.IsSharedVersion)
                Semaphore.Release();

            lock (Lock)
            {
                Utils.EncodeFormat($"{currentDirectory}/video.avi", $"{currentDirectory}/video.mp4");
            }

            this.Video = File.ReadAllBytes($"{currentDirectory}/video.mp4");

            if (File.Exists($"{currentDirectory}/video.mp4"))
                File.Delete($"{currentDirectory}/video.mp4");
        }

        private void ToBeUnique()
        {
            var q = Variables.IsSharedVersion ? Random.Next(90, 100) : Random.Next(60, 80);

            using (MemoryStream outStream = new MemoryStream())
            {
                using (MemoryStream inStream = new MemoryStream(this.Image))
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        imageFactory.Load(inStream)
                            .Brightness(Random.Next(-10, 20))
                            .Contrast(Random.Next(-10, 20))
                            //.BackgroundColor(GetColor())
                            .Tint(GetColor())
                            //.Vignette(GetColor())
                            .Quality(q)
                            .Format(new JpegFormat())
                            .Save(outStream);
                    }
                }

                this.Image = outStream.ToArray();
            }
        }

        private void RandomResize()
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (MemoryStream inStream = new MemoryStream(this.Image))
                {
                    Bitmap bmp = new Bitmap(inStream);
                    var w = bmp.Width;
                    var h = bmp.Height;

                    var a = 80;
                    if (w > 630) a = 100;
                    else if (w > 650) a = 130;
                    else if (w > 700) a = 180;
                    else if (w > 900) a = 200;

                    // Resize a whole image
                    a = Random.Next(a * -1, a);
                    bmp = Resize(bmp, w + a, h + a);

                    // Random resize w / h
                    a = 18;
                    w = bmp.Width;
                    h = bmp.Height;

                    bmp = Resize(bmp, w + Random.Next(a * -1, a / 2), h + Random.Next(a * -1, a));

                    var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    var myEncoder = Encoder.Quality;
                    var myEncoderParameters = new EncoderParameters(1);

                    var q = Variables.IsSharedVersion ? Random.Next(90, 95) : Random.Next(40, 70);

                    var myEncoderParameter = new EncoderParameter(myEncoder, q);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    bmp.Save(outStream, jpgEncoder, myEncoderParameters);
                    bmp.Dispose();
                }

                this.Image = outStream.ToArray();
            }
        }

        private static Bitmap Resize(Bitmap image, double width, double height)
        {
            Bitmap temp = new Bitmap((int)width, (int)height);
            using (Graphics gr = Graphics.FromImage(temp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(image, new Rectangle(0, 0, (int)width, (int)height));
            }

            return temp;
        }

        private static Color GetColor()
        {
            // List of colors
            List<Color> colors = new List<Color> { Color.AliceBlue, Color.Azure, Color.Beige, Color.BlanchedAlmond, Color.Cornsilk, Color.FloralWhite, Color.Honeydew, Color.Ivory, Color.Lavender, Color.LavenderBlush, Color.LemonChiffon, Color.LightCyan, Color.LightYellow, Color.Linen, Color.MintCream, Color.MistyRose, Color.OldLace, Color.PaleGoldenrod, Color.PapayaWhip, Color.PeachPuff, Color.Pink, Color.SeaShell, Color.Snow };

            // Set a random transparency
            var color = Color.FromArgb(Random.Next(220, 256), colors[Random.Next(0, colors.Count - 1)]);

            return color;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public void Update()
        {
            this.UpdateSize();
            this.UpdateCaption();
        }
    }
}
