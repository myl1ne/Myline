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
    public class IONodeAdaptiveTemplate:IONodeAdaptive
    {
        public IONodeAdaptiveTemplate(Point2D inputDim, Point2D outputDim)
            : base(inputDim, outputDim)
        {
        }

        protected override void bottomUpAdaptation()
        {

        }
        protected override void topDownAdaptation()
        {

        }

        protected override void bottomUp()
        {

        }

        protected override void topDown()
        {

        }

        /// <summary>
        /// Combine adapted clones of this node by fusing their modifications.
        /// If possible.
        /// </summary>
        /// <param name="bag">A bag containing all the adapted nodes</param>
        public override void fuse(IEnumerable<IONodeAdaptive> bag)
        {
            throw new NotImplementedException("The fuse mechanism has not been implemented for this node type.");
        }

    }
}
