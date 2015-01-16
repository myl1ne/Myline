﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
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

        /// <summary>
        /// Combine adapted clones of this node by fusing their modifications.
        /// If possible.
        /// </summary>
        /// <param name="bag">A bag containing all the adapted nodes</param>
        public virtual void fuse(IEnumerable<IONodeAdaptive> bag)
        {
            throw new NotImplementedException("The fuse mechanism has not been implemented for this node type.");
        }

    }
}
