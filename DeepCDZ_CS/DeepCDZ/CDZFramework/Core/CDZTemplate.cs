using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework.Core
{
    class CDZTemplate:CDZ
    {

        public CDZTemplate():base()
        {

        }
        
        //---------------------------------------------------------//
        /// <summary>
        /// Should be called once all the calls to AddModality have been done.
        /// </summary>
        public override void configure()
        {

        }
        //---------------------------------------------------------//
        #region Convergence
        protected override void preConvergence()
        {

        }

        protected override void convergenceFrom(Modality mod)
        {

        }
        protected override void postConvergence()
        {

        }
        public override float GetConfidence()
        {
            return 0.0f;
        }
        #endregion

        //---------------------------------------------------------//

        #region Divergence
        protected override void preDivergence()
        {

        }

        protected override void divergenceTo(Modality mod)
        {

        }
        protected override void postDivergence()
        {

        }
        #endregion
        //---------------------------------------------------------//

        #region Training
        //---------------------------------------------------------//

        public override void Train()
        { }
        #endregion


        //---------------------------------------------------------//
        #region Receptive Fields Plotting
        public override int computeBestExpert(Modality m, float[] value)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Modality, float[]> getReceptiveField(int expertIndex)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
