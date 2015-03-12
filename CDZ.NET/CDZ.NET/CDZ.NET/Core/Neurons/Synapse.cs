using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core.Neurons
{
    /// <summary>
    /// A weighted connection between two neurons
    /// </summary>
    class Connection
    {
        /// <summary>
        /// The neuron that is sending activity through this connection
        /// </summary>
        public Neuron source;

        /// <summary>
        /// The neuron that is sending receiving activity through this connection
        /// </summary>
        public Neuron target;

        /// <summary>
        /// The weight of the connection
        /// </summary>
        public double weight;

        /// <summary>
        /// Creates a connection between two neurons, with a given weight
        /// </summary>
        /// <param name="source">Source neuron</param>
        /// <param name="target">Target neuron</param>
        /// <param name="weight">Desired weight</param>
        public Connection(Neuron source, Neuron target, double weight)
        {
            this.source = source;
            this.target = target;
            this.weight = weight;
        }

        /// <summary>
        /// Creates a connection between two neurons, with a random weight in a given range
        /// </summary>
        /// <param name="source">Source neuron</param>
        /// <param name="target">Target neuron</param>
        /// <param name="lowWeightBoundary">Low boundary for the random weight (default 0.0)</param>
        /// <param name="highWeightBoundary">High boundary for the random weight (default 1.0)</param>
        public Connection(Neuron source, Neuron target, double lowWeightBoundary = 0.0, double highWeightBoundary = 1.0)
        {
            this.source = source;
            this.target = target;
            this.weight = MathHelpers.Rand.NextDouble()*(highWeightBoundary - lowWeightBoundary) + lowWeightBoundary;
        }
    }
}
