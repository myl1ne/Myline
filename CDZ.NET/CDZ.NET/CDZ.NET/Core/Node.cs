using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// The base class to represent a Signal.
    /// </summary>
    public class Node
    {
        public Signal output;

        public Node(Point2D outputDim)
        {
            output = new Signal((int)outputDim.X, (int)outputDim.Y);
        }
    }
}
