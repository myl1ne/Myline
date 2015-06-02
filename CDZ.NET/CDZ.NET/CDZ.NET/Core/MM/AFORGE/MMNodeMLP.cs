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
    public class MMNodeMLP:MMNode
    {
        ActivationNetwork network;
        ParallelResilientBackpropagationLearning teacher;

        int[] hiddenLayers;

        private double initialStep = 0.0125;
        public double sigmoidAlphaValue = 2.0;
        //public double learningRate = 0.1;

        public event EventHandler stuckOnDataset;

        //Try to unstuck the network
        private double lastErrorInEpoch;
        private int noChangeInErrorCount=0;

        public MMNodeMLP(Point2D outputDim, params int[] hiddenLayers)
            : base(outputDim)
        {
            this.hiddenLayers = hiddenLayers;

            //All those event handlers are optional
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;  
            onEpoch += HandleEpoch;
            onBatchStart += HandleBatchStart;
            stuckOnDataset += MMNodeMLP_stuckOnDataset;
        }

        void MMNodeMLP_stuckOnDataset(object sender, EventArgs e)
        {
            network.Randomize();
            teacher.Reset(initialStep);
        }


        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);

            int[] hiddenWithOutput = new int[hiddenLayers.Count()+1];
            hiddenLayers.CopyTo(hiddenWithOutput, 0);
            hiddenWithOutput[hiddenLayers.Count()] = InputCount;

            network = new ActivationNetwork(new SigmoidFunction(sigmoidAlphaValue), InputCount, hiddenWithOutput);
            teacher = new ParallelResilientBackpropagationLearning(network);

            // set learning rate and momentum
            teacher.Reset(initialStep);
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

            //Distribute it over the Signal
            setConcatenatedModalities(null, output);

            //Proceed to learning
            if (!learningLocked)
            {
                teacher.Run(realSignal, realSignal);
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

            double AFError = teacher.RunEpoch(samples, samples);

            //Run manually a base class epoch to have the exact same error measurement as other algos
            //learningLocked = true;
            //base.Epoch(trainingSet, out modalitiesMeanSquarredError, out globalMeanSquarred);
            //learningLocked = false;

            //HACK to remove ----------------------------------------
            globalMeanSquarred = AFError;
            modalitiesMeanSquarredError = new Dictionary<Signal, double>();
            foreach (Signal s in modalities)
                modalitiesMeanSquarredError[s] = AFError;
            // ----------------------------------------

            //Deal with stuck issues

            if (globalMeanSquarred == lastErrorInEpoch)
            {
                noChangeInErrorCount++;
            }
            else
            {
                lastErrorInEpoch = globalMeanSquarred;
                noChangeInErrorCount = 0;
            }

            if (noChangeInErrorCount > 10)
            {
                if (stuckOnDataset != null)
                {
                    stuckOnDataset(this, null);
                    noChangeInErrorCount = 0;
                }
            }
        }
    }
}
