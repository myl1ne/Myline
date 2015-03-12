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

    }
}
