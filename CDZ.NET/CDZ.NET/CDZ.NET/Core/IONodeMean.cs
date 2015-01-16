using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// An IONode that compute the mean (bottomup) or duplicate (topdown)
    /// </summary>
    public class IONodeMean:IONode
    {
        public IONodeMean(Point2D inputDim):base(inputDim, new Point2D(1,1))
        {
        }

        /// <summary>
        /// Calculate the mean of all the inputs
        /// </summary>
        protected override void bottomUp()
        {
            output.x[0, 0] = 0.0;
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    output.x[0, 0] += input.x[xI, yI];
                }
            }
            output.x[0, 0] /= (double)(input.Width * input.Height);
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
