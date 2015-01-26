using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
    public class Signal
    {
        public double[,] reality;
        public double[,] prediction;
        public int Width { get { return reality.GetLength(0); } }
        public int Height { get { return reality.GetLength(1); } }
        public Point2D Size { get { return new Point2D(Width, Height); } }

        public Signal(int w, int h)
        {
            reality = new double[w, h];
            prediction = new double[w, h];
        }

    }
}
