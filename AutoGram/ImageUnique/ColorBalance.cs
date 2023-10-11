using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.ImageUnique
{
    class ColorBalance
    {
        public static void Change(Bitmap image, UInt32[,] pixel, int percent)
        {
            int rand = Image.Random.Next(percent * -1, percent);

            UInt32 R;
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    R = ColorBalance.ColorBalance_R(pixel[i, j], rand);
                    Image.FromOnePixelToBitmap(i, j, R);
                }

            rand = Image.Random.Next(percent * -1, percent / 2);

            UInt32 G;
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    G = ColorBalance.ColorBalance_G(pixel[i, j], rand);
                    Image.FromOnePixelToBitmap(i, j, G);
                }

            rand = Image.Random.Next(percent * -1, percent);

            UInt32 B;
            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                {
                    B = ColorBalance.ColorBalance_B(pixel[i, j], rand);
                    Image.FromOnePixelToBitmap(i, j, B);
                }
        }

        private static UInt32 ColorBalance_R(UInt32 point, int N)
        {
            int R;
            int G;
            int B;

            R = (int)(((point & 0x00FF0000) >> 16) + N * 128 / 100);
            G = (int)((point & 0x0000FF00) >> 8);
            B = (int)(point & 0x000000FF);

            if (R < 0) R = 0;
            if (R > 255) R = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

        private static UInt32 ColorBalance_G(UInt32 point, int N)
        {
            int R;
            int G;
            int B;

            R = (int)((point & 0x00FF0000) >> 16);
            G = (int)(((point & 0x0000FF00) >> 8) + N * 128 / 100);
            B = (int)(point & 0x000000FF);

            if (G < 0) G = 0;
            if (G > 255) G = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

        private static UInt32 ColorBalance_B(UInt32 point, int N)
        {
            int R;
            int G;
            int B;

            R = (int)((point & 0x00FF0000) >> 16);
            G = (int)((point & 0x0000FF00) >> 8);
            B = (int)((point & 0x000000FF) + N * 128 / 100);

            if (B < 0) B = 0;
            if (B > 255) B = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }
    }
}
