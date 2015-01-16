using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// An IONode that compute the min (bottomup) or duplicate (topdown)
    /// </summary>
    public class IONodeMin:IONode
    {
        public IONodeMin(Point2D inputDim)
            : base(inputDim, new Point2D(1, 1))
        {
        }

        /// <summary>
        /// Calculate the mean of all the inputs
        /// </summary>
        protected override void bottomUp()
        {
            output.x[0, 0] = double.PositiveInfinity;
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    output.x[0, 0] = Math.Min(output.x[0, 0], input.x[xI, yI]);
                }
            }
        }

        /// <summary>
        /// Simply duplicate the output value to all the inputs
        /// </summary>
        protected override void topDown()
        {
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    input.x[xI, yI] = output.x[0, 0];
                }
            }
        }

    }
}
