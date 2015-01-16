using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// The base class to handle all bidirectional IO transformations
    /// </summary>
    public class IONode
    {
        public Signal input;
        public Signal output;
        public event EventHandler onBottomUp;
        public event EventHandler onTopDown;
        public event EventHandler onResized;

        public IONode(Point2D inputDim, Point2D outputDim)
        {
            input = new Signal((int)inputDim.X, (int)inputDim.Y);
            output = new Signal((int)outputDim.X, (int)outputDim.Y);
        }

        /// <summary>
        /// Update the output signal based on the current input signal
        /// </summary>
        public void BottomUp()
        {
            bottomUp();
            if (onBottomUp != null)
                onBottomUp(this, null);
        }

        protected virtual void bottomUp()
        {

        }

        /// <summary>
        /// Try to reconstruct the input signal based on the current output signal
        /// </summary>
        public void TopDown()
        {
            topDown();
            if (onTopDown != null)
                onTopDown(this, null);
        }

        protected virtual void topDown()
        {

        }

        /// <summary>
        /// Redimension the input and output signals.  Creates new objects for the signals, be carefull with references.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        protected void Resize(Point2D inputDim, Point2D outputDim)
        {
            input = new Signal((int)inputDim.X, (int)inputDim.Y);
            output = new Signal((int)outputDim.X, (int)outputDim.Y);

            if (onResized != null)
                onResized(this, null);
        }
    }
}
