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
    public class MMNodeMatLab:MMNode
    {
        /// <summary>
        /// COM object to communicate with matlab
        /// </summary>
        MLApp.MLApp matlab; 

        public MMNodeMatLab(Point2D outputDim, string scriptPath)
            : base(outputDim)
        {
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;

            //Initialise Matlab
            //var activationContext = Type.GetTypeFromProgID("matlab.application.single");
            //var matlab = (MLApp.MLApp)Activator.CreateInstance(activationContext);
            //Console.WriteLine(matlab.Execute("1+2"));
            matlab = new MLApp.MLApp(); 
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
            foreach(Signal s in modalities)
            {
                matlab.PutWorkspaceData(labelsModalities[s] + "_real", "base", s.reality);
                matlab.PutWorkspaceData(labelsModalities[s] + "_prediction", "base", s.reality);
            }
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
    }
}
