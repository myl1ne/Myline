using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CDZNET.GUI
{
    public static class GeneralExtensions
    {

        public static void fromBitmap(this CDZNET.Core.Signal signal, Bitmap bmp, bool useReality)
        {
            Bitmap tmp;

            double[,] signalToUse = useReality ? signal.reality : signal.prediction;

            //Rescale if needed
            if (bmp.Width != signal.Width || bmp.Height != signal.Height)
            {
                tmp = new Bitmap(bmp, new Size(signal.Width, signal.Height));
            }
            else
            {
                tmp = bmp;
            }

            for (int i = 0; i < tmp.Width; i++)
            {
                for (int j = 0; j < tmp.Height; j++)
                {
                    Color px = tmp.GetPixel(i, j);
                    float value = ColorHelper.RGB2GRAYf(px) / 255.0f;
                    signalToUse[i, j] = value;
                }
            }
        }

        public static Bitmap toBitmap(this CDZNET.Core.Signal signal, bool useReality, Color a, Color b)
        {
            double min = 0.0;
            double max = 1.0;
            double range = max - min;

            double[,] signalToUse = useReality ? signal.reality : signal.prediction;

            Bitmap bmp = new Bitmap(signal.Width, signal.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb); ;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //Color color = ColorInterpolator.InterpolateBetween(Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 0), Color.FromArgb(255, 0, 0), (array[y * bmp.Width + x] - min) / (range));
                    Color color = ColorHelper.InterpolateBetween(a, b, (signalToUse[i, j] - min) / (range));
                    bmp.SetPixel(i, j, color);
                }
            }
            //bmp.Save("D:/Logs/grmbl.bmp");
            return bmp;
        }

        public static Bitmap toBitmap(this CDZNET.Core.Signal signal, bool useReality, Color a, Color b, Color c)
        {
            double min = 0.0;
            double max = 1.0;
            double range = max - min;

            double[,] signalToUse = useReality ? signal.reality : signal.prediction;

            Bitmap bmp = new Bitmap(signal.Width, signal.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb); ;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = ColorHelper.InterpolateBetween(a, b, c, (signalToUse[i, j] - min) / (range));
                    bmp.SetPixel(i, j, color);
                }
            }
            //bmp.Save("D:/Logs/grmbl.bmp");
            return bmp;
        }

    }
}
