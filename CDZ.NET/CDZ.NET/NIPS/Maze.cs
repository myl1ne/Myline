using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIPS
{
    class Maze
    {
        public double[,] AdjMatrix { get; private set; }
        public double[,] DistMatrix { get; private set; }

        public int StatesCount {get{return AdjMatrix.GetLength(0);}}

        public List<int> ValidMoves(int state)
        {
            List<int> vm = new List<int>(StatesCount);
            for (int i = 0; i < StatesCount; i++)
			{
			    if (AdjMatrix[state,i] != 0.0)
                    vm.Add(i);
			}
            return vm;
        }

        public Maze(double[,] adjMatrix)
        {
            if (adjMatrix.GetLength(0) != adjMatrix.GetLength(1))
                throw new Exception("The matrix needs to be squared.");
            this.AdjMatrix = adjMatrix.Clone() as double[,];
            ComputeDistMatrix();
        }

        public List<Sequence> GenerateRandomSequences(int count, int lenght)
        {
            Random rand = new Random();
            List<Sequence> seqs = new List<Sequence>();
            for (int i = 0; i < count; i++)
            {
                Sequence s = new Sequence();
                int start = rand.Next(StatesCount);
                s.Add(start);
                while (s.Count < lenght)
                {
                    List<int> neighboors = ValidMoves(s.Last());
                    int next = neighboors[rand.Next(neighboors.Count)];
                    while (s.Count > 2 && next == s[s.Count - 2])
                    {
                        next = neighboors[rand.Next(neighboors.Count)];
                    }
                    s.Add(next);
                }
                //Console.WriteLine("Rnd Seq = " + s.ToString());
                seqs.Add(s);
            }
            return seqs;
        }

        void ComputeDistMatrix()
        {
            List<int> path = new List<int>();
            DistMatrix = new double[StatesCount, StatesCount];

            //Initialise
            for (int i = 0; i < StatesCount; i++)
            {
                for (int j = 0; j < StatesCount; j++)
                {
                    if (i == j)
                        DistMatrix[i, j] = 0.0;
                    else
                        DistMatrix[i, j] = double.PositiveInfinity;
                }                
            }
            //For each node
            for (int source = 0; source < StatesCount; source++)
            {
                List<int> unvisitedNodes = new List<int>();
                for (int node = 0; node < StatesCount; node++)
                {
                    unvisitedNodes.Add(node);
                }

                while(unvisitedNodes.Count>0)
                {
                    int u = unvisitedNodes.First();
                    foreach (int node in unvisitedNodes)
	                {
		                if (DistMatrix[source,node]<DistMatrix[source,u])
                        {
                            u = node;
                        }
	                }
                    unvisitedNodes.Remove(u);

                    List<int> neighbors = ValidMoves(u);
                    foreach (int v in neighbors)
                    {
                        double alt = DistMatrix[source, u] + 1;
                        if(alt<DistMatrix[source,v])
                            DistMatrix[source, v] = alt;
                    }
                }
            }

            ////Print
            //Console.WriteLine();
            //for (int i = 0; i < StatesCount; i++)
            //{
            //    for (int j = 0; j < StatesCount; j++)
            //    {
            //        Console.Write(DistMatrix[i, j] + "\t");
            //    }
            //    Console.WriteLine();
            //}
        }
    }
}
