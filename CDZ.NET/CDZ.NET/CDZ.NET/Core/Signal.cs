using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    public class Signal
    {
        public double[,] x;
        public int Width { get { return x.GetLength(0); } }
        public int Height { get { return x.GetLength(1); } }
        public Point2D Size { get { return new Point2D(Width, Height); } }

        public Signal(int w, int h)
        {
            x = new double[w, h];
        }
    }
}
