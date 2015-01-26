using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// An IONode that compute the max (bottomup) or duplicate (topdown)
    /// </summary>
    public class IONodeKernel : IONode
    {
        double[,] kernel;

        public IONodeKernel( double [,] kernel)
            : base( new Point2D(kernel.GetLength(0), kernel.GetLength(1)), new Point2D(1, 1))
        {
            this.kernel = kernel.Clone() as double[,];
        }

        /// <summary>
        /// Calculate the mean of all the inputs
        /// </summary>
        protected override void bottomUp()
        {
            output.prediction[0, 0] = 0.0;
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    output.prediction[0, 0] += input.reality[xI, yI] * kernel[xI, yI];
                }
            }
            output.prediction[0, 0] = Math.Max(0.0, Math.Min(1.0, output.prediction[0, 0]));
        }

        /// <summary>
        /// WRONG OPERATION, divide by the kernel values ? o_O
        /// </summary>
        protected override void topDown()
        {
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    input.prediction[xI, yI] = output.reality[0, 0] / kernel[xI, yI];
                }
            }
            input.prediction[0, 0] = Math.Max(0.0, Math.Min(1.0, input.prediction[0, 0]));
        }

    }
}
