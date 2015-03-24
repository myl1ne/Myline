using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VowelWorldModel
{
    class Cell
    {
        public int X = -1;
        public int Y = -1;

        public int shape = 0;

        public double frequency = 3.0f;

        public double orientation = 0.0f;

        public double colorValue = 0.0f;
        public double[] colorCode = new double[4]{0,0,0,0};

        public Color colorFromValue
        {
            get
            {
                Color c;
                if (colorValue < 1.0 / 4.0)
                    c = Color.Blue;
                else if (colorValue < 1.0 / 2.0)
                    c = Color.Green;
                else if (colorValue < 3.0 / 4.0)
                    c = Color.Red;
                else
                    c = Color.Yellow;
                return c;
            }
        }

        public Color colorFromCode
        {
            get
            {
                return getColorFromCode(colorCode);
            }
        }

        static public Color getColorFromCode(double[] code)
        {
                int max = 0;
                for (int i = 0; i < 4; i++)
                    if (code[i] > code[max])
                        max = i;
                Color c;
                if (max == 0)
                    c = Color.Blue;
                else if (max == 1)
                    c = Color.Green;
                else if (max == 2)
                    c = Color.Red;
                else
                    c = Color.Yellow;
                return c;
        }

        public double[] orientationCode
        {
            get
            {
                return getOrientationCode(orientation);
            }
        }
        public static double[] getOrientationCode(double value)
        {
            double[] code = new double[2];
            //0 = [0,0]
            if (value < 45.0)
            {
                code[0] = 0.0;
                code[1] = value / 45.0;
                //45 = [0,1]
            }
            else if (value < 90.0)
            {
                code[0] = (value - 45) / (90.0 - 45.0);
                code[1] = 1.0;
                //90 = [1,1]
            }
            else if (value < 135.0)
            {
                code[0] = 1.0;
                code[1] = 1.0 - (value - 90.0) / (135.0 - 90.0);
                //135 = [1,0]
            }
            else
            {
                code[0] = 1.0 - (value - 135.0) / (180.0 - 135.0);
                code[1] = 0.0;
                //180 = [0,0]
            }
            return code;
        }
        public static double getOrientationFromCode(double[] code)
        {
            double value;
            if (code[0]<=code[1]) //<90
            {
                value = code[0] * 45.0 + code[1] * 45;
            }
            else
            {
                value = 90.0 + (1-code[0]) * 45.0 + (1-code[1]) * 45;
            }
            return value;
        }

    }
}
