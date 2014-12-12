using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace CDZFramework.Core
{
    public class Modality
    {
        public string tag = "";

        public int Size { get { return realValues.Count(); } }
        private float[] realValues;
        public float[] RealValues { get { return realValues; } set { realValues = value; if (realValueUpdated != null) realValueUpdated(this, null); } }

        private float[] predictedValues;
        public float[] PredictedValues { get { return predictedValues; } set { predictedValues = value; if (predictedValuesUpdated != null) predictedValuesUpdated(this, null); } }

        public event EventHandler realValueUpdated;
        public event EventHandler predictedValuesUpdated;
        public CDZ parent { get; internal set; }

        public Modality(int size)
        {
            realValues = new float[size];
            predictedValues = new float[size];
        }

        public static void fromBitmap(float[] array, Bitmap bmp)
        {
            Bitmap tmp;
            //Rescale if needed
            if (bmp.Width * bmp.Height != array.Length)
            {
                tmp = new Bitmap(bmp, new Size((int)Math.Sqrt(array.Length), (int)Math.Sqrt(array.Length)));
            }
            else
            {
                tmp = bmp;
            }

            for (int x = 0; x < tmp.Width; x++)
            {
                for (int y = 0; y < tmp.Height; y++)
                {
                    Color px = tmp.GetPixel(x, y);
                    float value = 0.2126f * px.R + 0.7152f * px.G + 0.0722f * px.B;
                    array[y*tmp.Width+x] = value / 255.0f;
                }
            }
        }

        public static Bitmap toBitmap(float[] array)
        {
            double min = 0.0;
            double max = 1.0;
            double range = max - min;

            Bitmap bmp = new Bitmap((int)Math.Sqrt(array.Length), (int)Math.Sqrt(array.Length), System.Drawing.Imaging.PixelFormat.Format24bppRgb); ;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    //Color color = ColorInterpolator.InterpolateBetween(Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 0), Color.FromArgb(255, 0, 0), (array[y * bmp.Width + x] - min) / (range));
                    Color color = ColorInterpolator.InterpolateBetween(Color.White, Color.Black, (array[y * bmp.Width + x] - min) / (range)); bmp.SetPixel(x, y, color);
                    bmp.SetPixel(x, y, color);
                }
            }
            //bmp.Save("D:/Logs/grmbl.bmp");
            return bmp;
        }
    }
}
