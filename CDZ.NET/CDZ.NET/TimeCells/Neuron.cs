using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    [Serializable]
    class Neuron
    {
        public double activity = 0.0;
        public double previousActivity = 0.0;
        public double incomingCurrent = 0.0;

        public Dictionary<Neuron, double> ecInputs = new Dictionary<Neuron, double>();
        public Dictionary<Neuron, double> lateralInputs = new Dictionary<Neuron, double>();
        public Neuron()
        {

        }

        public void PreUpdate(double ecInfluence, double lateralInfluence)
        {
            //Update from EC, distance
            double ecCurrent = 0.0;
            foreach (Neuron n in ecInputs.Keys)
            {
                ecCurrent += 1.0 - Math.Abs(n.activity - ecInputs[n]);
            }
            ecCurrent /= ecInputs.Count();

            //Update from lateral, distance
            double lateralCurrent = 0.0;
            foreach (Neuron n in lateralInputs.Keys)
            {
                lateralCurrent += Math.Abs(n.activity * lateralInputs[n]);
            }
            lateralCurrent /= lateralInputs.Count();

            //Calculate the total current
            incomingCurrent = (ecCurrent * ecInfluence + lateralCurrent * lateralInfluence) / (ecInfluence + lateralInfluence);
        }

        public void EffectiveUpdate()
        {
            previousActivity = activity;
            activity = incomingCurrent > 0.5 ? 1.0 : 0.0; //Treshold
        }

        public void Train(double learningRate = 0.1)
        {
            if (activity == 1.0)
            {
                foreach (Neuron n in ecInputs.Keys.ToList())
                {
                    ecInputs[n] += learningRate * (n.activity - ecInputs[n]);
                    //incomingWeights[n] = Math.Min(Math.Max(-1.0, incomingWeights[n]), 1.0);
                }

                foreach (Neuron n in lateralInputs.Keys.ToList())
                {
                    if (n.activity == 1.0)
                        lateralInputs[n] += learningRate * (n.activity - lateralInputs[n]);
                    lateralInputs[n] -= 0.0001;
                    lateralInputs[n] = Math.Min(Math.Max(0.0, lateralInputs[n]), 1.0);
                }
            }
        }
    }
}
