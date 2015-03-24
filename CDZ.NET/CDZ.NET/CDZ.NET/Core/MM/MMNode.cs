using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// A multimodal node.
    /// </summary>
    public class MMNode: Node
    {
        public List<Signal> modalities;
        public Dictionary<Signal, double> modalitiesInfluence;
        public Dictionary<string, Signal> modalitiesLabels;
        public Dictionary<Signal, string> labelsModalities;
        public bool learningLocked = false;

        #region Events
        public event EventHandler onConvergence;
        public event EventHandler onDivergence;

        public delegate void BatchHandler(int maximumEpoch, double MSEStopCriterium);
        public event BatchHandler onBatchStart;
        public delegate void EpochHandler(int currentEpoch, int maximumEpoch, Dictionary<Signal, double> modalitiesMSE, double MSE);
        public event EpochHandler onEpoch;
        #endregion

        public MMNode(Point2D outputDim)
            : base(outputDim)
        {
            modalities = new List<Signal>();
            modalitiesInfluence = new Dictionary<Signal, double>();
            modalitiesLabels = new Dictionary<string, Signal>();
            labelsModalities = new Dictionary<Signal, string>();
        }

        public virtual void addModality(Signal s, string label = null)
        {
            modalities.Add(s);
            modalitiesInfluence[s] = 1.0f;
            if (label == null)
            {
                label = "unknown_" + modalities.Count;
            }

            labelsModalities[s] = label;
            modalitiesLabels[label] = s;
        }

        public void Cycle()
        {
            Converge();
            Diverge();
        }

        /// <summary>
        /// Implementation of the convergence operation.
        /// </summary>
        protected virtual void converge() { }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected virtual void diverge() { }

        /// <summary>
        /// Read the real input of all modalities and create an internal representation
        /// </summary>
        public void Converge() { converge(); if (onConvergence != null) onConvergence(this, null); }

        /// <summary>
        /// Read the current internal representation and predict every modality from it
        /// </summary>
        public void Diverge() { diverge(); if (onDivergence != null) onDivergence(this, null); }

        /// <summary>
        /// Given a subset of active signals, calculate the prediction on all signals.
        /// Basically works by setting the influence of active signals to 1 and the other to 0. Then produces a convergence/divergence
        /// operation without propagating the events (to avoid triggering the rest of the network)
        /// </summary>
        /// <param name="activeSignals">For each modality the prediction</param>
        /// <returns></returns>
        public Dictionary<Signal, double[,]> Predict(List<Signal> activeSignals)
        {
            bool preLock = learningLocked;
            learningLocked = true;
            Dictionary<Signal, double[,]> predictions = new Dictionary<Signal, double[,]>();
            Dictionary<Signal, double> previousInfluences = new Dictionary<Signal, double>(modalitiesInfluence);

            //Set the influences
            foreach (Signal s in modalities)
            {
                if (activeSignals.Contains(s))
                {
                    modalitiesInfluence[s] = 1.0;
                }
                else
                {
                    modalitiesInfluence[s] = 0.0;
                }
            }

            //Do the prediction cycle
            converge();
            diverge();

            foreach (Signal s in modalities)
            {
                predictions[s] = s.prediction.Clone() as double[,];
            }
            //Reset the influences
            modalitiesInfluence = previousInfluences;
            learningLocked = preLock;
            return predictions;
        }

        /// <summary>
        /// Given a subset of active signals, calculate the prediction error on all signals.
        /// Basically works by setting the influence of active signals to 1 and the other to 0. Then produces a convergence/divergence
        /// operation without propagating the events (to avoid triggering the rest of the network)
        /// </summary>
        /// <param name="activeSignals">For each modality the error matrix</param>
        /// <returns></returns>
        public Dictionary<Signal, double[,]> Evaluate(List<Signal> activeSignals)
        {
            Dictionary<Signal, double[,]> errors = Predict(activeSignals);           

            //Compute the errors
            foreach (Signal s in modalities)
            {
                errors[s] = s.ComputeError();
            }

            return errors;
        }


        public void Epoch(List<Dictionary<Signal, double[,]> > trainingSet, out Dictionary<Signal, double> modalitiesMeanSquarredError, out double globalMeanSquarred)
        {
            modalitiesMeanSquarredError = new Dictionary<Signal, double>();     
            foreach(Signal s in modalities)
                modalitiesMeanSquarredError[s] = 0.0;

            foreach(Dictionary<Signal, double[,]> sample in trainingSet)
            {
                //assign the modalities
                foreach(Signal s in modalities)
                    s.reality = sample[s].Clone() as double[,];
                
                //Process
                Converge();
                Diverge();

                //Compute error
                foreach (Signal s in modalities)
                    modalitiesMeanSquarredError[s] += s.ComputeMeanSquarredError();
            }

            globalMeanSquarred = 0.0;
            foreach (Signal s in modalities)
            {
                modalitiesMeanSquarredError[s] /= trainingSet.Count;
                globalMeanSquarred += modalitiesMeanSquarredError[s];
            }
            globalMeanSquarred /= modalities.Count;
        }

        /// <summary>
        /// Run a batch (i.e a given number of epochs or min MSE reached)
        /// </summary>
        /// <param name="trainingSet">The training set to be used</param>
        /// <param name="maximumEpochs">The maximum number of epochs to run</param>
        /// <param name="stopCritMSE">An optional MSE criterium for stopping</param>
        /// <returns>The number of epoch ran when the batch stopped</returns>
        public int Batch(List<Dictionary<Signal, double[,]> > trainingSet, int maximumEpochs, double stopCritMSE = 0.0)
        {
            if (onBatchStart != null)
                onBatchStart(maximumEpochs, stopCritMSE);

            double lastEpochMSE = double.PositiveInfinity;
            Dictionary<Signal, double> modalitiesMSE;
            
            int i = 0;
            for (; i < maximumEpochs && lastEpochMSE>stopCritMSE; i++)
			{
			    Epoch(trainingSet, out modalitiesMSE, out lastEpochMSE);
                if (onEpoch!=null)
                    onEpoch(i, maximumEpochs, modalitiesMSE, lastEpochMSE);
			}
            return i;
        }

    }
}
