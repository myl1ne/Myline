using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core.Neurons
{
    class Neuron
    {
        public double inputCurrent = 0.0;
        public double activity = 0.0;
        public double previousActivity = 0.0;

        public Dictionary<string, List<Connection> > inputs;

        public Neuron()
        {
            inputs = new Dictionary<string, List<Connection> >();
        }

        /// <summary>
        /// Add an existing connection to the input list.
        /// ERROR IN DESIGN : This should only be called by the Connection class.
        /// </summary>
        /// <param name="c">The connection</param>
        public void addInput(Connection c)
        {
            if (c.target != this)
                throw new Exception("Target of connection and input are differents.");

            if (!inputs.ContainsKey(c.tag))
                inputs[c.tag] = new List<Connection>();

            inputs[c.tag].Add(c);
        }
    }
}
