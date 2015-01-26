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
    class MMNode
    {
        public List<Signal> modalities;
        public Dictionary<Signal, double> modalitiesInfluence;
        public Dictionary<string, Signal> modalitiesLabels;
        public MMNode()
        {
            modalities = new List<Signal>();
            modalitiesInfluence = new Dictionary<Signal, double>();
            modalitiesLabels = new Dictionary<string, Signal>();
        }

        public virtual void addModality(Signal s, string label = null)
        {
            modalities.Add(s);
            modalitiesInfluence[s] = 1.0f;
            if (label != null)
                modalitiesLabels[label] = s;
        }

        public void Cycle()
        {
            Converge();
            Diverge();
        }

        /// <summary>
        /// Read the real input of all modalities and create an internal representation
        /// </summary>
        protected virtual void Converge() { }

        /// <summary>
        /// Read the current internal representation and predict every modality from it
        /// </summary>
        protected virtual void Diverge() { }
    }
}
