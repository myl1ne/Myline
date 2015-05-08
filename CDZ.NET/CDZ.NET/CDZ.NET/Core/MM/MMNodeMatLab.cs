using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
#if MATLAB
    /// <summary>
    /// A multimodal node template.
    /// </summary>
    public class MMNodeMatLab:MMNode
    {
        /// <summary>
        /// COM object to communicate with matlab. We use a singleton approach to avoid consuming many licences.
        /// </summary>
        static MLApp.MLApp matlab;

        private string scriptDir;
        public string matlabScriptsDirectory
        {
            get { return scriptDir; }
            set { scriptDir = value; matlab.Execute(@"cd " + scriptDir); }
        }

        //The name of the variable refering to this Node in matlab
        private string name;

        public MMNodeMatLab(Point2D outputDim,string nodeName, string scriptsPath, string scriptConvergenceDivergence)
            : base(outputDim)
        {
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;

            //Initialise Matlab ()
            if (matlab == null)
            {
                var activationContext = Type.GetTypeFromProgID("matlab.application.single");
                matlab = (MLApp.MLApp)Activator.CreateInstance(activationContext);
            }

            //Switch to the directory containing matlab scripts
            matlabScriptsDirectory = scriptsPath;

            name = nodeName;
            //Initialise a few variables
            matlab.Execute(name + ".modalities = {}");
            matlab.Execute(name + ".modalitiesNamesIndex = containers.Map");
        }

        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);

            //Append to the list of modalities
            //matlab.Execute(name + ".modalities{"+modalities.IndexOf(s)+"} = []");
            string queryCreateName = name + ".modalitiesNamesIndex(\'" + labelsModalities[s] + "\') = " + (modalities.IndexOf(s)+1);//length("+name + ".modalitiesNames)";
            matlab.Execute(queryCreateName);

            //Create the related variables
            updateInMatlab(s);
        }

        /// <summary>
        /// Return the stem variable name used in matlab for a given modality
        /// </summary>
        /// <returns></returns>
        private string getRootVariableName(Signal s)
        {
            return name + ".modalities{" + (modalities.IndexOf(s)+1) + "}";
        }

        /// <summary>
        /// Propagate the information of a modality to its sister variable in matlab
        /// </summary>
        /// <param name="s"></param>
        private void updateInMatlab(Signal s)
        {
            string stemName = getRootVariableName(s);

            matlab.PutWorkspaceData("buffer", "base", labelsModalities[s]);
            matlab.Execute(stemName + ".name = buffer");

            matlab.PutWorkspaceData("buffer", "base", s.reality);
            matlab.Execute(stemName + ".reality = buffer");

            matlab.PutWorkspaceData("buffer", "base", s.prediction);
            matlab.Execute(stemName + ".prediction = buffer");

            matlab.PutWorkspaceData("buffer", "base", modalitiesInfluence[s]);
            matlab.Execute(stemName + ".influence = buffer");
        }

        /// <summary>
        /// Implementation of the convergence operation, basically just transfer the content of modalities to their sisters variables in matlab.
        /// </summary>
        protected override void converge() 
        { 
            foreach(Signal s in modalities)
            {
                updateInMatlab(s);
            }
            //matlab.Execute("disp(\'cycle\')");
        }

        /// <summary>
        /// Implementation of the divergence operation, call the matlab script that is expected to give predictions.
        /// Pull the predictions from the sister variables to C# structure
        /// </summary>
        protected override void diverge()
        {
            //Do the computation
            matlab.Execute(name +" = dummyConvergenceDivergence(" + name + ")");

            //Copy back the prediction
            foreach (Signal s in modalities)
            {
                Object sPrediction = null;
                matlab.Execute("buffer = "+getRootVariableName(s) + ".prediction");
                matlab.GetWorkspaceData("buffer", "base", out sPrediction);
                s.prediction = ObjectCopier.Clone(sPrediction) as double[,];
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
    }
#endif
}
