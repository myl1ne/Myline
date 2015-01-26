using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    [Serializable]
    class Pool
    {
        List<Neuron> neurons = new List<Neuron>();
        public List<Neuron> inputNeurons;

        public Pool(int inputSize, int size = 100)
        {
            Random rand = new Random();

            //Create neurons
            inputNeurons = new List<Neuron>();
            for (int i = 0; i < inputSize; i++)
            {
                inputNeurons.Add(new Neuron());
            }

            for (int i = 0; i < size; i++)
            {
                neurons.Add(new Neuron());
            }

            //Create IO connections
            double IOconnectionProbability = 1.0;
            for (int i = 0; i < size; i++)
            {
                if (rand.NextDouble() < IOconnectionProbability)
                {
                    for (int j = 0; j < inputSize; j++)
                    {

                        neurons[i].ecInputs.Add(inputNeurons[j], rand.NextDouble() );
                    }
                }
            }
            //Create lateral connections
            double lateralConnectionProbability = 1.0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (rand.NextDouble() < lateralConnectionProbability)
                    {
                        neurons[i].lateralInputs.Add(neurons[j], rand.NextDouble() );
                    }
                }
            }
        }

        public void setInput(double[] vector)
        {
            for (int i = 0; i < inputNeurons.Count(); i++)
            {
                inputNeurons[i].activity = vector[i];
            }
        }

        public double error(ref string errorMessage)
        {    
            double e = 0.0;
            double[] predictedVector = new double[inputNeurons.Count()];
            for (int i = 0; i < inputNeurons.Count(); i++)
            {
                predictedVector[i] = 0.0;
                int activeNodes = 0;
                foreach (Neuron n in neurons)
                {
                    if (n.activity == 1.0)
                    {
                        predictedVector[i] += n.ecInputs[inputNeurons[i]];
                        activeNodes++;
                    }
                }
                predictedVector[i] /= activeNodes;
                //predictedVector[i] = predictedVector[i] > 0.5 ? 1.0 : 0.0; //Treshold
                e += Math.Pow(inputNeurons[i].activity - predictedVector[i], 2.0);
                errorMessage += inputNeurons[i].activity + "(" + predictedVector[i].ToString(String.Format("{0}{1}", "F", 4)) + ") \t";
            }
            return Math.Sqrt(e);
        }

        public void Update(double ecWeight, double lateralWeight, bool learning)
        {
            //Reflect
            int reflexion = 2;
            for (int i = 0; i < reflexion; i++)
            {
                
                foreach (Neuron n in neurons)
                {
                    n.PreUpdate(ecWeight, lateralWeight);
                }

                foreach (Neuron n in neurons)
                {
                    n.EffectiveUpdate();
                }

                //Track the winner and always turn it on (to avoid 0 nodes active)
                Neuron winner = null;
                foreach (Neuron n in neurons)
                {
                    if (winner == null || winner.incomingCurrent<n.incomingCurrent)
                    {
                        winner = n;
                    }
                }
                winner.activity = 1.0;
            }

            //Train output
            if (learning)
            {
                foreach (Neuron n in neurons)
                {
                    n.Train(0.01);
                }
            }
        }
    }
}
