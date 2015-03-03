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
            labelsModalities[s] = label;

            if (label != null)
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
    }
}
