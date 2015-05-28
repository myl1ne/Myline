using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Networks;
using AForge.Neuro;
using CDZNET.Helpers;
using AForge.Neuro.Learning;
using Accord.Neuro.Learning;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// An IO node that adaptes itself after each BottomUp or TopDown call.
    /// Uses the SOM algorithm. 
    /// The dimension of the output is either 2 (position of the winner in [0,1] or the dimension of the 
    /// map itself.
    /// </summary>
    public class IONodeAFMLP : IONodeAdaptive
    {
        private ActivationNetwork bottomUpNet;
        private ParallelResilientBackpropagationLearning bottomUpTeacher;
        private ActivationNetwork topDownNet;
        private ParallelResilientBackpropagationLearning topDownTeacher;
        private double initialStep = 0.0125;
        public double sigmoidAlphaValue = 2.0;

        /// <summary>
        /// Constructs a new SOM node.
        /// </summary>
        /// <param name="inputDim">The dimension of the input</param>
        /// <param name="mapSize">The dimension of the map</param>
        /// <param name="useOnlyWinnerAsOutput">Should only the position of the winner be used as an output. False means the whole map activity will be used.</param>
        public IONodeAFMLP(Point2D inputDim, Point2D outputDim, int[] hiddenLayersBottomUp, int[] hiddenLayerTopDown)
            : base(inputDim, outputDim)
        {
            int inputCount = (int)inputDim.X * (int)inputDim.Y;
            int outputCount = (int)outputDim.X * (int)outputDim.Y;

            int[] hiddenWithOutput = new int[hiddenLayersBottomUp.Count() + 1];
            hiddenLayersBottomUp.CopyTo(hiddenWithOutput, 0);
            hiddenWithOutput[hiddenLayersBottomUp.Count()] = outputCount;
            bottomUpNet = new ActivationNetwork(new SigmoidFunction(sigmoidAlphaValue), inputCount, hiddenWithOutput);
            bottomUpTeacher = new ParallelResilientBackpropagationLearning(bottomUpNet);

            int[] hiddenWithInput = new int[hiddenLayerTopDown.Count() + 1];
            hiddenLayerTopDown.CopyTo(hiddenWithInput, 0);
            hiddenWithInput[hiddenLayerTopDown.Count()] = inputCount;
            topDownNet = new ActivationNetwork(new SigmoidFunction(sigmoidAlphaValue), outputCount, hiddenWithInput);
            topDownTeacher = new ParallelResilientBackpropagationLearning(topDownNet);

            // set learning rate and momentum
            bottomUpTeacher.Reset(initialStep);
            topDownTeacher.Reset(initialStep);
        }

        /// <summary>
        /// For learning one step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="argsNull"></param>
        protected override void bottomUpAdaptation()
        {            
            double[] lInput = ArrayHelper.linearize(input.reality);
            double[] lOutput = ArrayHelper.linearize(output.reality);
            bottomUpTeacher.Run(lInput, lOutput);
        }

        /// <summary>
        /// For learning one step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="argsNull"></param>
        protected override void topDownAdaptation()
        {
            double[] lInput = ArrayHelper.linearize(input.reality);
            double[] lOutput = ArrayHelper.linearize(output.reality);
            topDownTeacher.Run(lOutput, lInput);
        }

        protected override void bottomUp()
        {
            double[] lInput = ArrayHelper.linearize(input.reality);
            double[] lOutput = bottomUpNet.Compute(lInput);
            ArrayHelper.unlinearize(lOutput, output.prediction);
        }

        protected override void topDown()
        {
            double[] lOutput = ArrayHelper.linearize(output.reality);
            double[] lInput = topDownNet.Compute(lOutput);
            ArrayHelper.unlinearize(lInput, input.prediction);
        }

        /// <summary>
        /// Combine adapted clones of this node by fusing their modifications.
        /// If possible.
        /// </summary>
        /// <param name="bag">An enumerable containing all the adapted nodes</param>
        public override void fuse(IEnumerable<IONodeAdaptive> bag)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Triggered after an epoch as been run during a batch call
        /// </summary>
        /// <param name="currentEpoch"> the current epoch</param>
        /// <param name="maximumEpoch"> the maximum epoch number to run</param>
        /// <param name="modalitiesMSE"> the MSE for each modality</param>
        /// <param name="MSE"> the average MSE</param>
        void HandleEpoch(int currentEpoch, int maximumEpoch, Dictionary<Signal, double> modalitiesMSE, double MSE)
        {
            //Here do something like learning rate adaptation, log, etc...
        }

        /// <summary>
        /// Triggered when a batch is started.
        /// </summary>
        /// <param name="maximumEpoch"></param>
        /// <param name="MSEStopCriterium"></param>
        void HandleBatchStart(int maximumEpoch, double MSEStopCriterium)
        {

        }

        /// <summary>
        /// Override the Epoch() method
        /// </summary>
        /// <param name="trainingSet"></param>
        /// <param name="modalitiesMeanSquarredError"></param>
        /// <param name="globalMeanSquarred"></param>
        public override void Epoch(List<KeyValuePair<double[,], double[,]>> trainingSet, out double outputMaxError, out double inputMaxError)
        {
            //Convert to Accord format
            double[][] samplesInput = new double[trainingSet.Count][];
            double[][] samplesOutput = new double[trainingSet.Count][];
            for (int i = 0; i < trainingSet.Count; i++)
            {
                samplesInput[i] = ArrayHelper.linearize(trainingSet[i].Key);
                samplesOutput[i] = ArrayHelper.linearize(trainingSet[i].Value);
            }

            bottomUpTeacher.RunEpoch(samplesInput, samplesOutput);
            //topDownTeacher.RunEpoch(samplesOutput, samplesInput);

            //Run manually a base class epoch to have the exact same error measurement as other algos
            learningLocked = true;
            base.Epoch(trainingSet, out outputMaxError, out inputMaxError);
            learningLocked = false;
        }
    }
}
