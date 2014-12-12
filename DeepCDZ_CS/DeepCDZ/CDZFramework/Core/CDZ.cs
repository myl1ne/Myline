using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework.Core
{
    public class CDZ
    {
        #region Events
        public event EventHandler onConvergence;
        public event EventHandler onDivergence;
        #endregion

        /// <summary>
        /// A list of the connected modalities
        /// </summary>
        internal List<Modality> modalities = new List<Modality>();

        /// <summary>
        /// A list of the relative influence of every connected modality
        /// </summary>
        internal Dictionary<Modality, float> modalitiesInfluences = new Dictionary<Modality, float>();
        
        /// <summary>
        /// A list of the relative influence of every connected modality
        /// </summary>
        internal Dictionary<Modality, float> modalitiesLearning = new Dictionary<Modality, float>();

        /// <summary>
        /// A placeholder for the subclasses to register their experts (see RFPlotting region)
        /// </summary>
        private List<object> experts = new List<object>();

        /// <summary>
        /// Builds a new CDZ
        /// </summary>
        protected CDZ()
        {
        }

        public Modality AddModality(int size, float influence = 1.0f, float learning = 1.0f)
        {
            Modality m = new Modality(size);
            m.parent = this;
            modalities.Add(m);
            modalitiesInfluences[m] = influence;
            modalitiesLearning[m] = learning;
            return m;
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Should be called once all the calls to AddModality have been done.
        /// </summary>
        public virtual void configure()
        {

        }

        //---------------------------------------------------------//
        #region Convergence
        protected virtual void preConvergence()
        {

        }

        protected virtual void convergenceFrom(Modality mod)
        {

        }
        protected virtual void postConvergence()
        {

        }

        /// <summary>
        /// Main Convergence operation
        /// </summary>
        public void Converge()
        {
            preConvergence();

            foreach (Modality mod in modalities)
                convergenceFrom(mod);

            postConvergence();

            //Propagate the event
            if (onConvergence!=null)
                onConvergence(this,null);
        }

        public virtual float GetConfidence()
        {
            return 0.0f;
        }
        #endregion

        //---------------------------------------------------------//

        #region Divergence
        protected virtual void preDivergence()
        {

        }

        protected virtual void divergenceTo(Modality mod)
        {

        }
        protected virtual void postDivergence()
        {

        }

        /// <summary>
        /// Main Divergence operation
        /// </summary>
        public void Diverge()
        {
            preDivergence();

            foreach (Modality mod in modalities)
                divergenceTo(mod);

            postDivergence();

            //Propagate the event
            if (onDivergence != null)
                onDivergence(this, null);
        }

        #endregion
        //---------------------------------------------------------//

        public virtual void Train()
        { }

        //---------------------------------------------------------//
        #region Receptive Fields Plotting
        protected object getExpertAtIndex(int index)
        {
            return experts[index];
        }
        public int getExpertIndex(object o)
        {
            return experts.IndexOf(o);
        }
        protected int registerExpert(object o)
        {
            experts.Add(o);
            return experts.Count();
        }

        /// <summary>
        /// Compute the best expert index for a given modality and the value for this modality.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int computeBestExpert(Modality m, float[] value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return how many experts are composing the CDZ
        /// </summary>
        /// <returns></returns>
        public int getExpertsNumber()
        {
            return experts.Count;
        }

        /// <summary>
        /// Get the multimodal receptive field of a given expert
        /// </summary>
        /// <param name="expertIndex"></param>
        /// <returns></returns>
        public virtual Dictionary<Modality, float[]> getReceptiveField(int expertIndex)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
