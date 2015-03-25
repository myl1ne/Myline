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
    /// <summary>
    /// A multimodal node making use of stacked RBM, wraps around Accord.NET.
    /// </summary>
    public class MMNodeDeepBeliefNetwork:MMNode
    {
        DeepBeliefNetwork network;
        DeepBeliefNetworkLearning teacher;

        int[] hiddenLayers;

        public double learningRate = 0.1;
        public double weightDecay = 0.001;
        public double momentum = 0.9;

        public MMNodeDeepBeliefNetwork(Point2D outputDim, int[] hiddenLayers)
            : base(outputDim)
        {
            this.hiddenLayers = hiddenLayers;

            //All those event handlers are optional
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;  
            onEpoch += HandleEpoch;
            onBatchStart += HandleBatchStart;
        }


        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);

            //Each time we add a modality the structure of the network changes
            int inputCount = 0;
            foreach(Signal mod in modalities)
                inputCount += mod.Width * mod.Height;

            //int[] wholeStructure = new int[hiddenLayers.Length + 2];
            //wholeStructure[0] = inputCount;
            //for (int i = 0; i < hiddenLayers.Length; i++)
            //    wholeStructure[i + 1] = hiddenLayers[i];
            //wholeStructure[hiddenLayers.Length+1] = inputCount;

            int[] wholeStructure = new int[hiddenLayers.Length + 1];
            wholeStructure[0] = inputCount;
            for (int i = 0; i < hiddenLayers.Length; i++)
                wholeStructure[i + 1] = hiddenLayers[i];
            wholeStructure[hiddenLayers.Length] = inputCount;

            network = new DeepBeliefNetwork(new BernoulliFunction(), inputCount, wholeStructure);

            new GaussianWeights(network).Randomize();
            network.UpdateVisibleWeights();

            for (int _layerIndex = 0; _layerIndex < wholeStructure.Length; _layerIndex++)
            {         
                teacher = new DeepBeliefNetworkLearning(network)
                {
                    Algorithm = (h, v, i) => new ContrastiveDivergenceLearning(h, v)
                    {
                        LearningRate = learningRate,
                        Momentum = momentum,
                        Decay = weightDecay,
                    },

                    LayerIndex = _layerIndex,
                };
            }
        }

        /// <summary>
        /// Get the modalities signals by concatenating them into 2 vectors.
        /// </summary>
        /// <param name="realSignal">The vector that will contain the real data</param>
        /// <param name="predictedSignal">The vector that will contain the predicted data</param>
        void getConcatenatedModalities(out double[] realSignal, out double[] predictedSignal)
        {
            double[] tmpRealSignal = new double[network.InputsCount];
            double[] tmpPredictedSignal = new double[network.InputsCount];

            int currentIndex = 0;
            foreach(Signal s in modalities)
            {
                ArrayHelper.ForEach(s.reality, false, (x,y) => 
                {
                    tmpRealSignal[currentIndex] = s.reality[x, y];
                    tmpPredictedSignal[currentIndex] = s.prediction[x, y];
                    currentIndex++;
                });
            }
            realSignal = tmpRealSignal;
            predictedSignal = tmpPredictedSignal;
        }

        /// <summary>
        /// Concatenate a training sample into a double vector
        /// </summary>
        /// <param name="realSignal">The vector that will contain the real data</param>
        /// <param name="predictedSignal">The vector that will contain the predicted data</param>
        double[] concatenateTrainingSample(Dictionary<Signal, double[,]> trainingSample)
        {
            double[] concatenated = new double[network.InputsCount];

            int currentIndex = 0;
            foreach (Signal s in modalities)
            {
                ArrayHelper.ForEach(s.reality, false, (x, y) =>
                {
                    concatenated[currentIndex] = trainingSample[s][x, y];
                    currentIndex++;
                });
            }
            return concatenated;
        }

        /// <summary>
        /// Set the modalities signals by unconcatenating vectors.
        /// </summary>
        /// <param name="realSignal">The vector containing real data (set to null for not touching the real part)</param>
        /// <param name="predictedSignal">The vector containing predicted data (set to null for not touching the predicted part)</param>
        void setConcatenatedModalities(double[] realSignal, double[] predictedSignal)
        {
            int currentIndex = 0;
            foreach (Signal s in modalities)
            {
                ArrayHelper.ForEach(s.reality, false, (x, y) =>
                {
                    if (realSignal != null)
                        s.reality[x, y] = realSignal[currentIndex];
                    if (predictedSignal != null)
                        s.prediction[x, y] = predictedSignal[currentIndex];
                    currentIndex++;
                });
            }
        }
        /// <summary>
        /// Implementation of the convergence operation.
        /// </summary>
        protected override void converge() 
        { 

        }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected override void diverge() 
        {
            //convert all the modalities in a single vector
            double[] realSignal; double[] predictedSignal;
            getConcatenatedModalities(out realSignal, out predictedSignal);

            //Generate the prediction
            double[] output = network.GenerateOutput(realSignal, 0);
            double[] networkPrediction = network.Reconstruct(output);

            //Distribute it over the Signal
            setConcatenatedModalities(null, networkPrediction);

            //Proceed to learning
            if (!learningLocked && learningRate>0)
            {
                //teacher.Run(realSignal);
                //network.UpdateVisibleWeights();
            }
        }

        /// <summary>
        /// Triggered after a convergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleConvergence(object o, EventArgs nullargs)
        {

        }

        /// <summary>
        /// Triggered after a divergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleDivergence(object o, EventArgs nullargs)
        {

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
        public override void Epoch(List<Dictionary<Signal, double[,]>> trainingSet, out Dictionary<Signal, double> modalitiesMeanSquarredError, out double globalMeanSquarred)
        {
            //Convert to Accord format
            double[][] samples = new double[trainingSet.Count][];
            for (int i = 0; i < trainingSet.Count; i++)
            {
                samples[i] = concatenateTrainingSample(trainingSet[i]);
            }

            globalMeanSquarred = teacher.RunEpoch(samples);
            globalMeanSquarred /= (samples.Count() * samples[0].Count());
            network.UpdateVisibleWeights();

            //We do not know the error specific to each modality, just set it to -1
            modalitiesMeanSquarredError = new Dictionary<Signal,double>();
            foreach(Signal s in modalities)
            {
                modalitiesMeanSquarredError[s] = -1;
            }
        }
    }
}
