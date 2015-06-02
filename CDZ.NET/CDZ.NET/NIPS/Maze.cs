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

        public int StatesCount {get{return AdjMatrix.GetLength(0);}}

        List<int> ValidMoves(int state)
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
            if (AdjMatrix.GetLength(0)!=AdjMatrix.GetLength(1))
                throw new Exception("The matrix needs to be squared.");
            this.AdjMatrix = adjMatrix.Clone() as double[,];
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
                Console.WriteLine("Rnd Seq = " + s.ToString());
                seqs.Add(s);
            }
            return seqs;
        }
    }
}
