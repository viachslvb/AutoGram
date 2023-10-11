using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.ImageUnique
{
    class Imaginary
    {
        private static readonly List<string> SmilesImages;
        private static int _counter;

        static Imaginary()
        {
            SmilesImages =  Directory.GetFiles(Settings.Basic.Image.ImaginaryPath + "/")
                            .Where(
                                fileImage =>
                                    Path.GetExtension(fileImage) == ".jpg" ||
                                    Path.GetExtension(fileImage) == ".png" ||
                                    Path.GetExtension(fileImage) == ".jpeg")
                            .ToList();
        }

        private static Bitmap GetSmile()
        {
            if (_counter >= SmilesImages.Count)
                _counter = 0;

            string smilePath = SmilesImages[_counter];
            _counter++;

            return new Bitmap(smilePath);
        }

        public static byte[] Draw(byte[] image)
        {
            Bitmap imageBitmap = new Bitmap(new MemoryStream(image));

            float width = imageBitmap.Width;
            float height = imageBitmap.Height;

            // Rectangle transparency
            int rectangleTransparencyMin = 100;
            int rectangleTransparencyMax = 200;
            int rectangleTransparency = Utils.Random.Next(rectangleTransparencyMin, rectangleTransparencyMax);

            // Rectangle height
            double rectangleHeightMin = 11.0;
            double rectangleHeightMax = 13.0;
            float rectangleHeight = (int)(Utils.Random.NextDouble() * (rectangleHeightMax - rectangleHeightMin) + rectangleHeightMin) * height / 100;

            // Rectangle margin top
            double rectangleMarginTopMin = 60.0;
            double rectangleMarginTopMax = 80.0; // !> rectangleHeightMax
            float rectangleMarginTop = (int)(Utils.Random.NextDouble() * (rectangleMarginTopMax - rectangleMarginTopMin) + rectangleMarginTopMin) * height / 100;

            Color rectangleColor = Settings.Basic.Image.UseImaginaryText
                ? Color.Black
                : Color.Black;

            // Draw rectangle
            using (var g = Graphics.FromImage(imageBitmap))
            {
                SolidBrush rectangleBrush = new SolidBrush(Color.FromArgb(rectangleTransparency, rectangleColor));

                g.FillRectangle(rectangleBrush, 0, rectangleMarginTop, width, rectangleHeight);
            }


            // --------------------------------------
            // Draw string
            // --------------------------------------

            if (Settings.Basic.Image.UseImaginaryText)
            {
                string text = Settings.Basic.Image.ImaginaryText.Replace(Variables.MessengerTemplate,
                    MediaObject.GetMessengerLogin());

                int fontSizeMin = 20;
                int fontSizeMax = 30;
                int fontSizePercent = (int) (Utils.Random.NextDouble() * (fontSizeMax - fontSizeMin) + fontSizeMin);
                int fontSize = fontSizePercent * (int) rectangleHeight / 100;

                //string fontFamily = FontNamesList[Random.Next(FontNamesList.Count)];
                string fontFamily = "Arial Black";

                Font font = new Font(fontFamily, fontSize);

                // String positionX in rectangle
                int stringMarginLeftMin = 5;
                int stringMarginLeftMax = 10;
                int stringMarginLeft =
                    (int)
                    (Utils.Random.NextDouble() * (stringMarginLeftMax - stringMarginLeftMin) + stringMarginLeftMin) *
                    (int) width / 100;

                // String positionY in rectangle
                int stringPosY = (100 - fontSizePercent) / 2 - 10;
                stringPosY = stringPosY * (int) rectangleHeight / 100;

                stringPosY = stringPosY + (int) rectangleMarginTop;

                using (var g = Graphics.FromImage(imageBitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawString(text, font, Brushes.White, stringMarginLeft, stringPosY);
                }
            }
            else
            {
                // Smile width in percent of height rectangle
                int smileHeightMin = 50;
                int smileHeightMax = 75;
                int smileHeightPercent = (int)(Utils.Random.NextDouble() * (smileHeightMax - smileHeightMin) + smileHeightMin);
                int smileHeight = smileHeightPercent * (int)rectangleHeight / 100;


                // Smile position Y
                int smilePosY = (100 - smileHeightPercent) / 2;
                smilePosY = smilePosY * (int)rectangleHeight / 100 + (int)rectangleMarginTop;


                // Smile between margin
                int smileMarginXMin = Settings.Basic.Image.ImaginaryMaxSmiles >= 6 ? 7 : 10;
                int smileMarginXMax = Settings.Basic.Image.ImaginaryMaxSmiles >= 6 ? 10 : 17;
                int smileMarginX = (int)(Utils.Random.NextDouble() * (smileMarginXMax - smileMarginXMin) + smileMarginXMin) * (int)width / 100 + smileHeight;


                // Draw rectangle
                using (var g = Graphics.FromImage(imageBitmap))
                {
                    int marginX = smileMarginX;
                    for (var i = 0;
                        i <
                        Utils.Random.Next(Settings.Basic.Image.ImaginaryMinSmiles,
                            Settings.Basic.Image.ImaginaryMaxSmiles);
                        i++)
                    {
                        Bitmap smile = GetSmile();
                        smile = Scale(smile, smileHeight);

                        g.DrawImage(smile, marginX, smilePosY, smileHeight, smileHeight);

                        marginX = marginX + smileMarginX;
                    }
                }
            }

            return Image.SaveToByte(imageBitmap);
        }

        private static Bitmap Scale(Bitmap image, double height)
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            var heightDifference = height * 100 / imageHeight;
            var width = heightDifference * imageWidth / 100;

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
    }
}
