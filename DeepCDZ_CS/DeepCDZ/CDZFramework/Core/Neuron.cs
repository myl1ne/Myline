using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework.Core
{
    public class Neuron
    {
        public float activity;
        public Dictionary<Modality, float[]> weights;

        public Neuron()
        {
            activity = 0.0f;
            weights = new Dictionary<Modality,float[]>();
        }

        public static float RFDistance(Neuron a, Neuron b)
        {
            double d = 0.0f;
            foreach(Modality m in a.weights.Keys)
            {
                float md = 0.0f;
                for (int i = 0; i < m.Size; i++)
                {
                    md += (float)Math.Pow(a.weights[m][i] - b.weights[m][i],2.0);
                }
                d+=Math.Sqrt(md)/m.Size;
            }
            return (float)(d/a.weights.Keys.Count);
        }
    }
}
