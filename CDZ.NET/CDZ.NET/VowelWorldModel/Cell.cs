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
    }
}
