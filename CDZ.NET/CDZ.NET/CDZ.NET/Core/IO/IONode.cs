using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// The base class to handle all bidirectional IO transformations
    /// </summary>
    public class IONode: Node
    {
        public Signal input;
        public event EventHandler onBottomUp;
        public event EventHandler onTopDown;
        public event EventHandler onResized;
        public delegate void BatchHandler(int maximumEpoch, double MSEStopCriterium);
        public event BatchHandler onBatchStart;
        public delegate void EpochHandler(int currentEpoch, int maximumEpoch, double outputMaxError, double inputMaxError);
        public event EpochHandler onEpoch;

        public bool learningLocked = false;

        public IONode(Point2D inputDim, Point2D outputDim):base(outputDim)
        {
            input = new Signal((int)inputDim.X, (int)inputDim.Y);
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

        public virtual void Epoch(List<KeyValuePair<double[,], double[,]>> trainingSet, out double outputMaxError, out double inputMaxError)
        {
            outputMaxError = 0.0;
            inputMaxError = 0.0;

            foreach (KeyValuePair<double[,], double[,]> sample in trainingSet)
            {
                //assign
                input.reality = sample.Key.Clone() as double[,];
                output.reality = sample.Value.Clone() as double[,];

                //Process
                BottomUp();
                TopDown();

                //Compute error
                inputMaxError += input.ComputeMaxAbsoluteError();
                outputMaxError += output.ComputeMaxAbsoluteError();
            }
            inputMaxError /= trainingSet.Count;
            outputMaxError /= trainingSet.Count;
        }

        /// <summary>
        /// Run a batch (i.e a given number of epochs or min MSE reached)
        /// </summary>
        /// <param name="trainingSet">The training set to be used</param>
        /// <param name="maximumEpochs">The maximum number of epochs to run</param>
        /// <param name="stopCritMSE">An optional MSE criterium for stopping</param>
        /// <returns>The number of epoch ran when the batch stopped</returns>
        public int Batch(List<KeyValuePair<double[,], double[,]>> trainingSet, int maximumEpochs, double stopCritMaxE = 0.0)
        {
            if (onBatchStart != null)
                onBatchStart(maximumEpochs, stopCritMaxE);

            double inputMaxError = double.PositiveInfinity;
            double outputMaxError = double.PositiveInfinity;

            int i = 0;
            for (; i < maximumEpochs && outputMaxError > stopCritMaxE; i++)
            {
                Epoch(trainingSet, out outputMaxError, out inputMaxError);
                if (onEpoch != null)
                    onEpoch(i, maximumEpochs, outputMaxError, inputMaxError);
            }
            return i;
        }
    }
}
