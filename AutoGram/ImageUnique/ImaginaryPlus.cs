using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageMagick;
using AutoGram.Storage.Model;

namespace AutoGram.ImageUnique
{
    enum AspectRatio
    {
        Portrait,
        Normal,
        NormalBig,
        Widescreen,
        WidescreenSmall
    }

    class ImaginaryPlus : IDisposable
    {
        private readonly MagickImage _image;
        private readonly AspectRatio _aspectRatio;

        private static readonly Random Random = new Random();

        private static readonly List<PixelInterpolateMethod> InterpolateMethods = new List<PixelInterpolateMethod>
        {
            PixelInterpolateMethod.Background,
            PixelInterpolateMethod.Bilinear, PixelInterpolateMethod.Blend,
            PixelInterpolateMethod.Catrom, PixelInterpolateMethod.Integer,
            PixelInterpolateMethod.Mesh, PixelInterpolateMethod.Nearest,
            PixelInterpolateMethod.Spline, PixelInterpolateMethod.Undefined
        };

        private static readonly List<Color> FillColors = new List<Color>
        {
            Color.Black, Color.FromArgb(42, 50, 58),  Color.FromArgb(39,55,71),  Color.CadetBlue,
            Color.Coral, Color.Crimson, Color.DarkCyan, Color.Crimson, Color.DarkSlateBlue,
            Color.DarkSlateGray, Color.DarkOliveGreen, Color.IndianRed, Color.LightSeaGreen, Color.Teal,
            Color.FromArgb(123, 4, 46)
        };

        private static readonly List<Color> StrokeColors = new List<Color>
        {
            Color.LavenderBlush, Color.Cornsilk, Color.AliceBlue, Color.Azure, Color.FloralWhite,
            Color.Gainsboro, Color.GhostWhite, Color.Honeydew, Color.Ivory,
            Color.Lavender
        };

        private static readonly List<Color> LineColors = new List<Color>
        {
            Color.Aqua, Color.Coral, Color.Crimson, Color.DarkCyan, Color.IndianRed, Color.LightSeaGreen,
            Color.DarkOrange, Color.Cyan, Color.DarkRed, Color.DeepPink, Color.HotPink,
            Color.DarkTurquoise, Color.DeepSkyBlue, Color.Gold, Color.GreenYellow,
            Color.IndianRed, Color.LightCoral, Color.LightGreen, Color.Orange,
            Color.OrangeRed, Color.Salmon, Color.LightSeaGreen, Color.Red
        };

        private static readonly List<string> FontsList;

        static ImaginaryPlus()
        {
            FontsList = Directory.GetFiles(Variables.FolderFonts).ToList();
        }

        public void PrepareDirectPhoto()
        {
            // Add noise

            // Image quality
            if (Settings.Advanced.Direct.MaxUniqueizePhoto)
            {
                Resize();
                Colorize();
                BrightnessContrast();
                Sharpen();
                AddNoise();
            }
            else
            {
                int r = Random.Next(25000, 50000);
                double noise = (double)r / 100000;
                _image.AddNoise(NoiseType.Gaussian, noise);
            }

            _image.Quality = Random.Next(90, 100);
        }

        public ImaginaryPlus(byte[] image, bool isProfilePicture = false, bool isDirectPhoto = false)
        {
            _image = new MagickImage(image);
            _aspectRatio = ViewAspectRatio();

            if (isDirectPhoto)
            {
                PrepareDirectPhoto();
                return;
            }

            Resize();

            if (!isProfilePicture && Settings.Advanced.Image.UseLiquidRescale)
            {
                _image.LiquidRescale(new Percentage(Settings.Advanced.Image.LiquidRescalePercentage));
            }

            if (!isProfilePicture && Settings.Advanced.Image.Lines.Draw
                && Settings.Advanced.Image.Lines.UseImaginary
                && Settings.Advanced.Image.Lines.IsBackground)
            {
                DrawLines(Settings.Advanced.Image.Lines);
            }

            if (isProfilePicture && Settings.Advanced.Image.ProfilePicture.Lines.Draw)
            {
                DrawLines(Settings.Advanced.Image.ProfilePicture.Lines);
            }

            if (!isProfilePicture && Settings.Advanced.Image.Text.Draw && Settings.Advanced.Image.Text.UseImaginary)
            {
                if (Settings.Advanced.Image.Text.Swirl)
                {
                    DrawText();
                    Swirl();
                }
                else
                {
                    Swirl();
                    DrawText();
                }
            }

            if (!isProfilePicture && Settings.Advanced.Image.Lines.Draw
                && Settings.Advanced.Image.Lines.UseImaginary
                && !Settings.Advanced.Image.Lines.IsBackground)
            {
                DrawLines(Settings.Advanced.Image.Lines);
            }

            if (isProfilePicture
                && Settings.Advanced.Image.Text.Swirl
                || !Settings.Advanced.Image.Text.Draw
                && Settings.Advanced.Image.Text.Swirl)
            {
                Swirl();
            }

            Colorize();
            BrightnessContrast();
            Sharpen();
            AddNoise();

            if (!isProfilePicture && Settings.Advanced.Image.Lines.Draw
                && !Settings.Advanced.Image.Lines.UseImaginary
                && Settings.Advanced.Image.Lines.IsBackground)
            {
                DrawLines(Settings.Advanced.Image.Lines);
            }

            if (!isProfilePicture && Settings.Advanced.Image.Blur.Use)
            {
                _image.Blur(Random.Next(
                    Settings.Advanced.Image.Blur.Radius.From,
                    Settings.Advanced.Image.Blur.Radius.To) + Random.NextDouble(),
                    Random.Next(
                        Settings.Advanced.Image.Blur.Sigma.From,
                        Settings.Advanced.Image.Blur.Sigma.To) + Random.NextDouble());
            }

            if (isProfilePicture && Settings.Advanced.Image.ProfilePicture.Blur.Use)
            {
                _image.Blur(Random.Next(
                    Settings.Advanced.Image.ProfilePicture.Blur.Radius.From,
                    Settings.Advanced.Image.ProfilePicture.Blur.Radius.To) + Random.NextDouble(),
                    Random.Next(
                        Settings.Advanced.Image.ProfilePicture.Blur.Sigma.From,
                        Settings.Advanced.Image.ProfilePicture.Blur.Sigma.To) + Random.NextDouble());
            }

            if (!isProfilePicture && Settings.Advanced.Image.Text.Draw && !Settings.Advanced.Image.Text.UseImaginary)
            {
                if (Settings.Advanced.Image.Text.Swirl)
                {
                    DrawText();
                    Swirl();
                }
                else
                {
                    Swirl();
                    DrawText();
                }
            }

            if (!isProfilePicture && Settings.Advanced.Image.Lines.Draw
                && !Settings.Advanced.Image.Lines.UseImaginary
                && !Settings.Advanced.Image.Lines.IsBackground)
            {
                DrawLines(Settings.Advanced.Image.Lines);
            }

            if (!isProfilePicture && Settings.Advanced.Image.AddKikProfile)
            {
                AddKikLogo();
            }

            _image.Quality = Settings.Advanced.Image.HighQuality
                ? Random.Next(90, 100)
                : Random.Next(40, 60);
        }

        public void AddKikLogo()
        {
            var kikImage = new MagickImage(Variables.ImageLogoKik);
            var width = GetPixelsByPercent(35, _image.Width);

            kikImage.Scale(width, width);
            int x = _image.Width / 2 - kikImage.Width / 2;
            int y = _image.Height / 2 - kikImage.Height / 2;

            _image.Composite(kikImage, x, y, CompositeOperator.Over);
        }

        public void DrawLines(ImageLines settings)
        {
            if (!settings.UseThickLines
                && !settings.UseThinLines)
                return;

            if (!settings.DrawLeftBottomSide
                && !settings.DrawTopRightSide)
                return;

            var linesImage = new MagickImage(new MagickColor(Color.Transparent), _image.Width, _image.Height);

            // Settings
            var useBothLinesType = settings.UseThickLines
                                   && settings.UseThinLines;

            // todo: pixelPercentage lineMargin

            int lineMargin = useBothLinesType
                ? Random.Next(0, 2) == 0
                    ? Random.Next(15, 40)
                    : Random.Next(150, 250)
                : settings.UseThinLines
                    ? Random.Next(15, 40)
                    : Random.Next(150, 250);

            int lineSize = (int)(lineMargin / 2.7);

            var opacityPercentage = new Percentage(Random.Next(
                settings.Opacity.From,
                settings.Opacity.To));

            int linesXCount = _image.Width / lineMargin;
            int linexYCount = _image.Height / lineMargin;

            var drawables = new Drawables();

            // todo: pixelPercentage randomDeviation

            // Top/Right position
            int randomDeviation = Random.Next(0, 250);

            // todo: colorLine settings

            bool isColorLine = false;
            var p1 = randomDeviation;
            var p2 = 0 - lineSize;
            var p3 = _image.Width - p1;
            var p4 = _image.Height + lineSize;

            if (settings.DrawTopRightSide)
            {
                for (var i = 0; i < linesXCount; i++)
                {
                    if (settings.UseColorfulLine)
                    {
                        var useColorfulLine = linesXCount > 10
                        ? Random.Next(0, 7) == 0
                        : Random.Next(0, 2) == 0;

                        if (!isColorLine && useColorfulLine)
                        {
                            var colorLine = LineColors[Random.Next(LineColors.Count)];

                            var colorfulLine = new Drawables();
                            colorfulLine.Line(p1, p2, p3, p4);
                            colorfulLine.StrokeWidth(lineSize);
                            colorfulLine.StrokeColor(colorLine);
                            colorfulLine.StrokeOpacity(opacityPercentage);
                            colorfulLine.FillColor(colorLine);
                            colorfulLine.FillOpacity(new Percentage(0));

                            linesImage.Draw(colorfulLine);
                            isColorLine = true;
                            continue;
                        }
                    }

                    drawables.Line(p1, p2, p3, p4);

                    p1 = p1 + lineMargin;
                    p3 = p3 + lineMargin;

                    if (p3 > _image.Width)
                        p4 = p4 - lineMargin;
                }
            }

            // Left/Bottom position

            if (settings.DrawLeftBottomSide)
            {
                p1 = randomDeviation > 10
                    ? randomDeviation - lineMargin
                    : 0 - lineMargin;

                p2 = randomDeviation > 10
                    ? 0 - lineSize
                    : lineMargin - lineSize;

                p3 = _image.Width - randomDeviation - lineMargin;
                p4 = _image.Height + lineSize;

                for (var i = 0; i < linexYCount; i++)
                {
                    drawables.Line(p1, p2, p3, p4);

                    if (p1 > 0)
                    {
                        p1 = p1 - lineMargin;
                        p2 = p2 - lineMargin;
                    }
                    else p1 = p1 - lineSize;

                    p3 = p3 - lineMargin;
                }
            }

            var color = StrokeColors[Random.Next(StrokeColors.Count)];

            drawables.StrokeWidth(lineSize);
            drawables.StrokeColor(color);
            drawables.StrokeOpacity(opacityPercentage);
            drawables.FillColor(color);
            drawables.FillOpacity(new Percentage(0));
            linesImage.Draw(drawables);

            linesImage.Swirl(Random.Next(-150, 150));

            _image.Composite(linesImage, CompositeOperator.Over);
        }

        public void Resize()
        {
            _image.Resize(new Percentage(Random.Next(90, 120) + Random.NextDouble()));
        }

        public void DrawText()
        {
            int fontSizePercent;
            int rectangleHeightPercent;

            switch (_aspectRatio)
            {
                case AspectRatio.Portrait:
                    fontSizePercent = 40;
                    rectangleHeightPercent = 6;
                    break;
                case AspectRatio.Normal:
                    fontSizePercent = 36;
                    rectangleHeightPercent = 8;
                    break;
                case AspectRatio.NormalBig:
                    fontSizePercent = 36;
                    rectangleHeightPercent = 7;
                    break;
                case AspectRatio.Widescreen:
                    fontSizePercent = 36;
                    rectangleHeightPercent = 8;
                    break;
                case AspectRatio.WidescreenSmall:
                    fontSizePercent = 38;
                    rectangleHeightPercent = 6;
                    break;
                default:
                    fontSizePercent = 34;
                    rectangleHeightPercent = 8;
                    break;
            }

            if (Settings.Advanced.Image.Text.LargeSize)
            {
                fontSizePercent = (int)(fontSizePercent * 1.3);
                rectangleHeightPercent = (int)(rectangleHeightPercent * 1.5);
            }

            // Draw rectangle
            var width = _image.Width;
            var height = _image.Height;

            // Rectangle transparency
            var rectangleTransparency = Settings.Advanced.Image.Text.RandomTrancparency
                ? new Percentage(Random.Next(70, 90))
                : new Percentage(70);

            // Rectangle height
            float rectangleHeight = GetPixelsByPercent(rectangleHeightPercent, height);

            // Rectangle margin top
            float rectangleMarginTop = Settings.Advanced.Image.Text.RandomPosition
                ? GetPixelsByPercent(Random.Next(50, 80), height)
                : GetPixelsByPercent(60, height);

            var fillColor = Settings.Advanced.Image.Text.Colorize
                ? FillColors[Random.Next(FillColors.Count)]
                : FillColors[0];

            var strokeColor = Settings.Advanced.Image.Text.Colorize
                ? StrokeColors[Random.Next(StrokeColors.Count)]
                : StrokeColors[0];

            _image.Draw(new Drawables()
                .FillColor(MagickColors.White)
                .FillColor(fillColor)
                .FillOpacity(rectangleTransparency)
                .Rectangle(0, rectangleMarginTop, width, rectangleMarginTop + rectangleHeight));

            int fontSize = fontSizePercent * (int)rectangleHeight / 100;

            float middleRectangle = rectangleHeight / 2 + (int)(fontSize / 2.4);
            int textPositionY = (int)rectangleMarginTop + (int)middleRectangle;

            int middleWidth = _image.Width / 2;

            string text = Settings.Advanced.Image.Text.Title.Replace(Variables.MessengerTemplate,
                    MediaObject.GetMessengerLogin());

            string fontFamily = Settings.Advanced.Image.Text.UseDifferentFonts
                ? FontsList[Random.Next(FontsList.Count)]
                : "Arial";

            var draw = new Drawables()
                .FontPointSize(fontSize)
                .Font(fontFamily)
                .StrokeColor(strokeColor)
                .StrokeOpacity(new Percentage(Random.Next(70, 90)))
                .FillColor(strokeColor)
                .TextAlignment(TextAlignment.Center)
                .Text(middleWidth, textPositionY, text);

            _image.Draw(draw);
        }

        public void Swirl()
        {
            int degrees = Random.Next(0, 2) == 1 ? Random.Next(-10, -2) : Random.Next(3, 15);
            var interpolateMethod = InterpolateMethods[Random.Next(InterpolateMethods.Count)];

            _image.Swirl(interpolateMethod, degrees);
        }

        public void Sharpen()
        {
            int r1 = Settings.Advanced.Image.HighImagine
                ? Random.Next(2, 5)
                : Random.Next(0, 4);

            int r2 = Settings.Advanced.Image.HighImagine
                ? Random.Next(8, 18)
                : Random.Next(0, 15);

            _image.Sharpen(r1 + Random.NextDouble(), r2 + Random.NextDouble());
        }

        public void AddNoise()
        {
            int r = Settings.Advanced.Image.HighImagine
                ? Random.Next(95000, 115000)
                : Random.Next(50000, 95000);

            if (Settings.Advanced.Image.Noise.Use)
            {
                r = Random.Next(Settings.Advanced.Image.Noise.From, Settings.Advanced.Image.Noise.To);
            }

            double noise = (double)r / 100000;

            _image.AddNoise(NoiseType.Gaussian, noise);
        }

        public void BrightnessContrast()
        {
            _image.BrightnessContrast(
                new Percentage(Random.Next(-12, 10)),
                new Percentage(Random.Next(-12, 12))
                );
        }

        public void Colorize()
        {
            MagickColor color = MagickColor.FromRgb(
                (byte)Random.Next(40, 180),
                (byte)Random.Next(40, 180),
                (byte)Random.Next(40, 180)
                );

            _image.Colorize(color, new Percentage(Random.Next(1, 10) + Random.NextDouble()));
        }

        public byte[] ToBytes()
        {
            return _image.ToByteArray();
        }

        private int GetPixelsByPercent(float percent, float size)
        {
            return (int)(percent * size / 100);
        }

        private int GetPercentByPixels(float pixels, float size)
        {
            return (int)(pixels * 100 / size);
        }

        public AspectRatio ViewAspectRatio()
        {
            var width = _image.Width;
            var height = _image.Height;

            if (width == height) return AspectRatio.Normal;

            if (width > height)
            {
                int difference = width - height;
                int differenceInPercent = GetPercentByPixels(difference, width);

                if (differenceInPercent < 5)
                {
                    if (width > 1000 || height > 1000)
                    {
                        return AspectRatio.NormalBig;
                    }

                    return AspectRatio.Normal;
                }

                return AspectRatio.Widescreen;
            }
            else
            {
                int difference = height - width;
                int differenceInPercent = GetPercentByPixels(difference, height);

                if (width < 550) return AspectRatio.WidescreenSmall;

                if (differenceInPercent < 5)
                {
                    if (width > 1000 || height > 1000)
                    {
                        return AspectRatio.NormalBig;
                    }

                    return AspectRatio.Normal;
                }

                return AspectRatio.Portrait;
            }
        }

        public void Dispose()
        {
            _image?.Dispose();
        }
    }
}
