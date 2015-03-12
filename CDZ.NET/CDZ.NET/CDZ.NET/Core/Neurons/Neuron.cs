using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core.Neurons
{
    class Neuron
    {
        List<Connection> outputs;
        List<Connection> inputs;

        public Neuron()
        {
            outputs = new List<Connection>();
        }
    }
}
