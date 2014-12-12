using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework.Core
{
    public class CDZ_ESOM : CDZ
    {
        public List<Neuron> neurons;
        Neuron winner = null;
        public Dictionary<Neuron, List<Neuron>> connections = new Dictionary<Neuron, List<Neuron>>();
        public int ConnectionsCount {get; private set;}
        #region Parameters
        float prunningTreshold = 0.3f;
        float tolerance = 0.01f;
        float learningRate = 0.05f;
        int newNodeConnectionCount = 2;
        #endregion

        public CDZ_ESOM(int initialNeuronsCount = 3)
            : base()
        {
            neurons = new List<Neuron>();
            ConnectionsCount = 0;
            for (int i = 0; i < initialNeuronsCount; i++)
            {
                Neuron n = new Neuron();
                neurons.Add(n);
                connections[n] = new List<Neuron>();
                registerExpert(n);
            }

            //Connect all of them
            foreach (Neuron n1 in neurons)
            {
                foreach(Neuron n2 in neurons)
                {
                    if (n1 != n2)
                    {
                        connections[n1].Add(n2);
                        ConnectionsCount++;
                    }
                }
            }
        }

        //---------------------------------------------------------//
        /// <summary>
        /// Should be called once all the calls to AddModality have been done.
        /// </summary>
        public override void configure()
        {
            foreach(Neuron n in neurons)
            {
                foreach(Modality m in modalities)
                {
                    n.weights[m] = new float[m.Size];
                    for (int i = 0; i < m.Size; i++)
                    {
                        n.weights[m][i] = (float)MathHelpers.Rand.NextDouble();
                    }
                }
            }
        }

        //---------------------------------------------------------//
        #region Convergence
        protected override void preConvergence()
        {
            foreach (Neuron n in neurons)
            {
                n.activity = 0.0f;
            }
        }

        protected override void convergenceFrom(Modality mod)
        {
            foreach (Neuron n in neurons)
            {
                float contribution = 0.0f;
                for (int i = 0; i < mod.Size; i++)
                {
                    contribution += 1.0f-(float)Math.Pow(mod.RealValues[i] - n.weights[mod][i], 2.0);
                }
                contribution /= mod.Size;
                n.activity += modalitiesInfluences[mod] * contribution;
            }
        }
        protected override void postConvergence()
        {
            //Divide by the influence sum and keep track of winner
            float influenceSum = modalitiesInfluences.Values.Sum();
            winner = null;
            foreach (Neuron n in neurons)
            {
                n.activity /= influenceSum;
                if (winner == null || n.activity > winner.activity)
                    winner = n;
            }

            //Check if we have to add a new node
            if (winner.activity<(1.0f-tolerance))
            {
                Neuron n = new Neuron();
                n.activity = 1.0f;
                registerExpert(n);
                foreach(Modality m in modalities)
                {
                    n.weights[m] = (float[])m.RealValues.Clone();
                }
                neurons.Add(n);
                connections[n] = new List<Neuron>();

                //Sort the neurons by their distance to this new one (here we could optimize by using the activity instead, which is already computed)
                List<Neuron> orderedNeighbors = new List<Neuron>();
                orderedNeighbors.AddRange(neurons);
                orderedNeighbors.OrderByDescending(o=>Neuron.RFDistance(n,o));

                //Create connection to the closest neurons
                for(int i=1; i<=newNodeConnectionCount && i<orderedNeighbors.Count-1;i++)
                {
                    connections[n].Add(orderedNeighbors[i]);
                    ConnectionsCount++;
                    connections[orderedNeighbors[i]].Add(n);
                    ConnectionsCount++;
                }

                Console.WriteLine("Added a new neuron with " + connections[n].Count + " connections.");
            }
        }
        #endregion

        //---------------------------------------------------------//

        #region Divergence
        protected override void preDivergence()
        {
        }

        protected override void divergenceTo(Modality mod)
        {
            for (int i = 0; i < mod.Size; i++)
            {
                mod.PredictedValues[i] = 0.0f;
            }

            float totalContribution = 0.0f;
            foreach (Neuron n in neurons)
            {
                if (considerForPrediction(n))
                {
                    for (int i = 0; i < mod.Size; i++)
                    {
                        mod.PredictedValues[i] += n.activity * n.weights[mod][i];
                    }

                    totalContribution += n.activity;
                }
            }

            for (int i = 0; i < mod.Size; i++)
            {
                mod.PredictedValues[i] /= totalContribution;
            }
        }

        protected override void postDivergence()
        {

        }
        #endregion
        //---------------------------------------------------------//

        bool considerForPrediction(Neuron n)
        {
            return n == winner;
            //return (n.activity > 1.0 - tolerance);
        }

        #region Training
        //---------------------------------------------------------//

        public override void Train()
        { 
            //Strenghten connections
            foreach(Neuron n1 in neurons)
            {
                if (winner == n1 || connections[n1].Contains(winner))
                {
                    foreach(Modality m in winner.weights.Keys)
                    {
                        for (int i = 0; i < winner.weights[m].Count(); i++)
                        {
                            float error = m.RealValues[i] - n1.weights[m][i];
                            float dW = learningRate *modalitiesLearning[m]*(-0.5f + MathHelpers.Sigmoid(n1.activity, 10.0f)) * error;
                            n1.weights[m][i] += dW;
                        }
                    }
                }
            }

            //Prunning step
            if (neurons.Count > 3)
            {
                List<Neuron> lonelyNeurons = new List<Neuron>();
                foreach (Neuron n1 in neurons)
                {
                    List<KeyValuePair<Neuron, Neuron>> toBePrunned = new List<KeyValuePair<Neuron, Neuron>>();
                    foreach (Neuron n2 in connections[n1])
                    {
                        float rfDistance = Neuron.RFDistance(n1, n2);
                        if (rfDistance >= prunningTreshold)
                        {
                            toBePrunned.Add(new KeyValuePair<Neuron, Neuron>(n1, n2));
                        }
                    }

                    foreach (var k in toBePrunned)
                    {
                        connections[k.Key].Remove(k.Value);
                        ConnectionsCount--;
                        connections[k.Value].Remove(k.Key);
                        ConnectionsCount--;
                        if (connections[k.Key].Count == 0)
                            lonelyNeurons.Add(k.Key);
                        if (connections[k.Value].Count == 0)
                            lonelyNeurons.Add(k.Value);
                    }
                }

                foreach (Neuron n in lonelyNeurons)
                {
                    if (neurons.Count > 3)
                    {
                        neurons.Remove(n);
                        connections.Remove(n);
                    }
                }
            }
        }
        #endregion

        #region Receptive Fields Plotting
        public override int computeBestExpert(Modality m, float[] value)
        {
            Neuron best = null;
            float bestDistance = 1.0f;
            foreach (Neuron n in neurons)
            {
                float distance = 0.0f;
                for (int i = 0; i < m.Size; i++)
                {
                    distance += 1.0f - (float)Math.Pow(value[i] - n.weights[m][i], 2.0);
                }
                distance /= m.Size;

                if (distance <= bestDistance)
                {
                    best = n;
                    bestDistance = distance;
                }
            }
            return getExpertIndex(best);
        }

        public override Dictionary<Modality, float[]> getReceptiveField(int expertIndex)
        {
            return (getExpertAtIndex(expertIndex) as Neuron).weights;
        }
        #endregion
    }
}
