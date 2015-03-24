using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// A multimodal node template.
    /// </summary>
    public class MMNodeTemplate:MMNode
    {
        public MMNodeTemplate(Point2D outputDim)
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
    }
}
