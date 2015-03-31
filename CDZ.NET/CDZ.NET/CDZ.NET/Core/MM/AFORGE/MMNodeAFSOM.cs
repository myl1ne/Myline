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
    /// A multimodal node making use of a SOM (single modality style), wraps around AForge.NET.
    /// </summary>
    public class MMNodeAFSOM:MMNode
    {
        DistanceNetwork network;
        SOMLearning teacher;

        int[] hiddenLayers;

        private double learningRate = 0.3;
        private int learningRadius = 3;
        double fixedLearningRate;
        double driftingLearningRate;

        public MMNodeAFSOM(Point2D outputDim)
            : base(outputDim)
        {
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

            network = new DistanceNetwork(inputCount, output.Width * output.Height);
            teacher = new SOMLearning(network, output.Width, output.Height);
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
            double[] output = network.Compute(realSignal);
            Neuron winner = network.Layers[0].Neurons[network.GetWinner()];
            double[] networkPrediction = new double[InputCount];
            for (int i = 0; i < InputCount; i++)
            {
                networkPrediction[i] = winner.Weights[i];
            }

            //Distribute it over the Signal
            setConcatenatedModalities(null, networkPrediction);

            //Proceed to learning
            if (!learningLocked && learningRate>0)
            {
                teacher.Run(realSignal);
            }

            //todo : copy the neuron activities to MNNode output
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
            //Reduce the learning rate / radius
            teacher.LearningRate = driftingLearningRate * (maximumEpoch - currentEpoch) / maximumEpoch + fixedLearningRate;
            teacher.LearningRadius = (double)learningRadius * (maximumEpoch - currentEpoch) / maximumEpoch;
        }

        /// <summary>
        /// Triggered when a batch is started.
        /// </summary>
        /// <param name="maximumEpoch"></param>
        /// <param name="MSEStopCriterium"></param>
        void HandleBatchStart(int maximumEpoch, double MSEStopCriterium)
        {
            fixedLearningRate = learningRate / 10;
            driftingLearningRate = fixedLearningRate * 9;
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

            //Epoch
            teacher.RunEpoch(samples);

            //Run manually a base class epoch without learning to have the exact same error measurement as other algos
            learningLocked = true;
            base.Epoch(trainingSet, out modalitiesMeanSquarredError, out globalMeanSquarred);
            learningLocked = false;
        }
    }
}
