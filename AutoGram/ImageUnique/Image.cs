using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace AutoGram.ImageUnique
{
    class Image
    {
        public static readonly Random Random = new Random();
        private static readonly int ColorBalancePercent = 25;
        private static readonly int BrightnessContrastPercent = 25;

        private static Bitmap _image;
        private static UInt32[,] _pixel;

        public static byte[] GetImage(string path)
        {
            _image = new Bitmap(path);

            //Flip();
            //Rotate(background);

            UniqueScale();
            //UniqueDrawLine();

            _pixel = new UInt32[_image.Height, _image.Width];
            for (int y = 0; y < _image.Height; y++)
                for (int x = 0; x < _image.Width; x++)
                    _pixel[y, x] = (UInt32)(_image.GetPixel(x, y).ToArgb());

            //Blur();
            //Sharpen();

            ChangeBrightnessContrast();
            ChangeColorBalance();

            return SaveToByte();
        }

        private static void Flip()
        {
            /*var x = Random.Next(0, 1);

            if (x == 0)*/
            _image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        public static byte[] Rotate(Bitmap photo, Bitmap background)
        {
            List<Color> colors = new List<Color>
            {
                Color.AliceBlue,
                Color.AntiqueWhite,
                Color.Azure,
                Color.Beige,
                Color.Bisque,
                Color.BlanchedAlmond,
                Color.BurlyWood,
                Color.Cornsilk,
                Color.DarkSalmon,
                Color.FloralWhite,
                Color.Gainsboro,
                Color.GhostWhite,
                Color.Ivory,
                Color.LemonChiffon,
                Color.Lavender,
                Color.LavenderBlush,
                Color.LightCyan,
                Color.LightGoldenrodYellow,
                Color.LightYellow
            };

            var color = colors[Random.Next(0, colors.Count - 1)];

            var angle = Random.Next(-3, 3);

            return SaveToByte(RotateImage(photo, angle, background, color));
        }

        private static Bitmap GetBitmapFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (var bmp = new Bitmap(ms))
                {
                    return bmp;
                }
            }
        }

        public static byte[] GetBackgroundImage()
        {
            //Sharpen();
            Blur();

            return SaveToByte();
        }

        public static Bitmap RotateImage(Bitmap bmp, float angle, Bitmap background, Color bkColor)
        {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;

            System.Drawing.Imaging.PixelFormat pf = default(System.Drawing.Imaging.PixelFormat);
            if (bkColor == Color.Transparent)
            {
                pf = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            }
            else
            {
                pf = bmp.PixelFormat;
            }

            float sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // this function takes radians
            float cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // this one too
            float newImgWidth = sin * bmp.Height + cos * bmp.Width;
            float newImgHeight = sin * bmp.Width + cos * bmp.Height;
            float originX = 0f;
            float originY = 0f;

            if (angle > 0)
            {
                if (angle <= 90)
                    originX = sin * bmp.Height;
                else
                {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * bmp.Width;
                }
            }
            else
            {
                if (angle >= -90)
                    originY = sin * bmp.Width;
                else
                {
                    originX = newImgWidth - sin * bmp.Height;
                    originY = newImgHeight;
                }
            }

            Bitmap newImg = new Bitmap((int)newImgWidth, (int)newImgHeight, pf);
            Graphics g = Graphics.FromImage(newImg);
            g.Clear(bkColor);
            g.DrawImage(background, 0, 0);
            g.TranslateTransform(originX, originY); // offset the origin to our calculated values
            g.RotateTransform(angle); // set up rotate
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(bmp, 0, 0); // draw the image at 0, 0
            g.Dispose();

            return newImg;
        }

        private static void ChangeColorBalance()
        {
            ColorBalance.Change(_image, _pixel, ColorBalancePercent);
        }

        private static void ChangeBrightnessContrast()
        {
            BrightnessContrast.Change(_image, _pixel, BrightnessContrastPercent);
        }

        private static void Sharpen()
        {
            var r = Random.Next(1, 2);

            for (var i = 0; i < r; i++)
                _pixel = Filter.matrix_filtration(_image.Width, _image.Height, _pixel, Filter.N1, Filter.Sharpness);

            FromPixelToBitmap();
        }

        private static void Blur()
        {
            var r = Random.Next(17, 30);

            for (var i = 0; i < r; i++)
                _pixel = Filter.matrix_filtration(_image.Width, _image.Height, _pixel, Filter.N2, Filter.Blur);

            FromPixelToBitmap();
        }

        private static void UniqueScale()
        {
            var width = _image.Width;
            var height = _image.Height;

            var alteration = 80;
            if (width > 630) alteration = 100;
            if (width > 650) alteration = 130;
            if (width > 700) alteration = 180;
            if (width > 900) alteration = 200;

            // scale size of image
            alteration = Random.Next(alteration * -1, alteration);
            _image = Scale(width + alteration, height + alteration);


            // scale width / height of image
            alteration = 18;
            width = _image.Width;
            height = _image.Height;

            _image = Scale(width + Random.Next(alteration * -1, alteration / 2), height + Random.Next(alteration * -1, alteration));
        }

        private static void UniqueDrawLine()
        {
            // Set a random points

            // Set x1
            const double marginLeftMin = 20.0;
            const double marginLeftMax = 99.0;
            var x1 = (int)(Random.NextDouble() * (marginLeftMax - marginLeftMin) + marginLeftMin) * _image.Width / 100;

            // Set y1
            const double marginTopMin = 20.0;
            const double marginTopMax = 99.0;
            var y1 = (int)(Random.NextDouble() * (marginTopMax - marginTopMin) + marginTopMin) * _image.Height / 100;

            // Set x2, y2
            int x2 = x1 - Random.Next(-20, 40);
            int y2 = y1 - Random.Next(-20, 40);

            Pen pen = new Pen(Color.White);

            DrawLine(pen, x1, y1, x2, y2);
        }

        private static void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            using (var graphics = Graphics.FromImage(_image))
            {
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private static Bitmap Scale(double width, double height)
        {
            Bitmap temp = new Bitmap((int)width, (int)height);
            using (Graphics gr = Graphics.FromImage(temp))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(_image, new Rectangle(0, 0, (int)width, (int)height));
            }

            return temp;
        }

        private static byte[] SaveToByte()
        {
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);

            var q = Settings.Basic.Image.LowQuality
                ? Random.Next(12, 23)
                : Random.Next(45, 70);

            var myEncoderParameter = new EncoderParameter(myEncoder, q);
            myEncoderParameters.Param[0] = myEncoderParameter;

            using (var stream = new MemoryStream())
            {
                _image.Save(stream, jpgEncoder, myEncoderParameters);
                return stream.ToArray();
            }
        }

        public static byte[] SaveToByte(Bitmap imageBitmap)
        {
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);

            var q = Settings.Basic.Image.LowQuality
                ? Random.Next(12, 23)
                : Random.Next(45, 70);

            var myEncoderParameter = new EncoderParameter(myEncoder, q);
            myEncoderParameters.Param[0] = myEncoderParameter;

            using (var stream = new MemoryStream())
            {
                imageBitmap.Save(stream, jpgEncoder, myEncoderParameters);
                return stream.ToArray();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public static void FromOnePixelToBitmap(int x, int y, UInt32 pixel)
        {
            _image.SetPixel(y, x, Color.FromArgb((int)pixel));
        }

        public static void FromPixelToBitmap()
        {
            for (int y = 0; y < _image.Height; y++)
                for (int x = 0; x < _image.Width; x++)
                    _image.SetPixel(x, y, Color.FromArgb((int)_pixel[y, x]));
        }
    }
}
