using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// An IO node that adaptes itself after each BottomUp or TopDown call.
    /// </summary>
    public class IONodeAdaptive:IONode
    {
        public IONodeAdaptive(Point2D inputDim, Point2D outputDim)
            : base(inputDim, outputDim)
        {
            this.onBottomUp += bottomUpAdaptation;
            this.onTopDown += topDownAdaptation;
        }

        public virtual void bottomUpAdaptation(object sender, EventArgs argsNull)
        {

        }
        public virtual void topDownAdaptation(object sender, EventArgs argsNull)
        {

        }

        protected override void bottomUp()
        {

        }

        protected override void topDown()
        {

        }

    }
}
