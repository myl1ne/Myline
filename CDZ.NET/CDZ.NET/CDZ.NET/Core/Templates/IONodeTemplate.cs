using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    public class IONodeTemplate:IONode
    {
        public IONodeTemplate(Point2D inputDim, Point2D outputDim)
            : base(inputDim, outputDim)
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
