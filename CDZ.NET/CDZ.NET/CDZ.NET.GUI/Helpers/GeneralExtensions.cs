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

        public static void fromBitmap(this CDZNET.Core.Signal signal, Bitmap bmp)
        {
            Bitmap tmp;
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
                    signal.x[i, j] = value;
                }
            }
        }

        public static Bitmap toBitmap(this CDZNET.Core.Signal signal)
        {
            double min = 0.0;
            double max = 1.0;
            double range = max - min;

            Bitmap bmp = new Bitmap(signal.Width, signal.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb); ;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //Color color = ColorInterpolator.InterpolateBetween(Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 0), Color.FromArgb(255, 0, 0), (array[y * bmp.Width + x] - min) / (range));
                    Color color = ColorHelper.InterpolateBetween(Color.Black, Color.White, (signal.x[i, j] - min) / (range));
                    bmp.SetPixel(i, j, color);
                }
            }
            //bmp.Save("D:/Logs/grmbl.bmp");
            return bmp;
        }
    }
}
