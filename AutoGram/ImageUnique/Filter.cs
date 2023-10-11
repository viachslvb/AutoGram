using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.ImageUnique
{
    struct RGB
    {
        public float R;
        public float G;
        public float B;
    }

    class Filter
    {
        public static UInt32[,] matrix_filtration(int W, int H, UInt32[,] pixel, int N, double[,] matryx)
        {
            int i, j, k, m, gap = (int)(N / 2);
            int tmpH = H + 2 * gap, tmpW = W + 2 * gap;
            UInt32[,] tmppixel = new UInt32[tmpH, tmpW];
            UInt32[,] newpixel = new UInt32[H, W];

            for (i = 0; i < gap; i++)
                for (j = 0; j < gap; j++)
                {
                    tmppixel[i, j] = pixel[0, 0];
                    tmppixel[i, tmpW - 1 - j] = pixel[0, W - 1];
                    tmppixel[tmpH - 1 - i, j] = pixel[H - 1, 0];
                    tmppixel[tmpH - 1 - i, tmpW - 1 - j] = pixel[H - 1, W - 1];
                }

            for (i = gap; i < tmpH - gap; i++)
                for (j = 0; j < gap; j++)
                {
                    tmppixel[i, j] = pixel[i - gap, j];
                    tmppixel[i, tmpW - 1 - j] = pixel[i - gap, W - 1 - j];
                }
  
            for (i = 0; i < gap; i++)
                for (j = gap; j < tmpW - gap; j++)
                {
                    tmppixel[i, j] = pixel[i, j - gap];
                    tmppixel[tmpH - 1 - i, j] = pixel[H - 1 - i, j - gap];
                }

            for (i = 0; i < H; i++)
                for (j = 0; j < W; j++)
                    tmppixel[i + gap, j + gap] = pixel[i, j];

            RGB colorOfPixel = new RGB();
            RGB colorOfCell;
            for (i = gap; i < tmpH - gap; i++)
                for (j = gap; j < tmpW - gap; j++)
                {
                    colorOfPixel.R = 0;
                    colorOfPixel.G = 0;
                    colorOfPixel.B = 0;
                    for (k = 0; k < N; k++)
                        for (m = 0; m < N; m++)
                        {
                            colorOfCell = CalculationOfColor(tmppixel[i - gap + k, j - gap + m], matryx[k, m]);
                            colorOfPixel.R += colorOfCell.R;
                            colorOfPixel.G += colorOfCell.G;
                            colorOfPixel.B += colorOfCell.B;
                        }

                    if (colorOfPixel.R < 0) colorOfPixel.R = 0;
                    if (colorOfPixel.R > 255) colorOfPixel.R = 255;
                    if (colorOfPixel.G < 0) colorOfPixel.G = 0;
                    if (colorOfPixel.G > 255) colorOfPixel.G = 255;
                    if (colorOfPixel.B < 0) colorOfPixel.B = 0;
                    if (colorOfPixel.B > 255) colorOfPixel.B = 255;

                    newpixel[i - gap, j - gap] = Build(colorOfPixel);
                }

            return newpixel;
        }

        public static RGB CalculationOfColor(UInt32 pixel, double coefficient)
        {
            RGB Color = new RGB();
            Color.R = (float)(coefficient * ((pixel & 0x00FF0000) >> 16));
            Color.G = (float)(coefficient * ((pixel & 0x0000FF00) >> 8));
            Color.B = (float)(coefficient * (pixel & 0x000000FF));
            return Color;
        }

        public static UInt32 Build(RGB colorOfPixel)
        {
            UInt32 Color;
            Color = 0xFF000000 | ((UInt32)colorOfPixel.R << 16) | ((UInt32)colorOfPixel.G << 8) | ((UInt32)colorOfPixel.B);
            return Color;
        }

        public const int N1 = 3;
        public static double[,] Sharpness = new double[N1, N1] {{-1, -1, -1},
                                                               {-1,  9, -1},
                                                               {-1, -1, -1}};

        public const int N2 = 3;
        public static double[,] Blur = new double[N1, N1] {{0.111, 0.111, 0.111},
                                                               {0.111, 0.111, 0.111},
                                                               {0.111, 0.111, 0.111}};
    }
}
