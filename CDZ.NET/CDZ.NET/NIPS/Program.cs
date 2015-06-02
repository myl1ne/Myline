using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Networks;
using AForge.Neuro;
using AForge.Neuro.Learning;
using Accord.Neuro.Learning;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NIPS
{
    [Serializable]
    class Sequence : List<int>
    {
        public Sequence() : base() { }
        public Sequence(Sequence s) : base(s) { }

        public String ToString()
        {
            string str = "";
            foreach (int item in this)
            {
                str += item + "->";
            }
            return str;
        }
    }

    struct Triplet
    {
        public int x0;
        public int x1;
        public int g;

        public String ToString()
        {
            return "G=" + g + "\tX0=" + x0 + "\tX1=" + x1;
        }
    }

    class Program
    {
        static List<int> ValidMoves(int state)
        {
            switch (state)
	        {
                case 0: return new List<int>(){1,5};
                case 1: return new List<int>(){0,2};
                case 2: return new List<int>(){1,3,6};
                case 3: return new List<int>(){2,4};
                case 4: return new List<int>(){3,7};
                case 5: return new List<int>(){0,8};
                case 6: return new List<int>(){2,10};
                case 7: return new List<int>(){4,12};
                case 8: return new List<int>(){5,9,13};
                case 9: return new List<int>(){8,10};
                case 10: return new List<int>(){6,9,11,14};
                case 11: return new List<int>(){10,12};
                case 12: return new List<int>(){7,11,15};
                case 13: return new List<int>(){8,16};
                case 14: return new List<int>(){10,18};
                case 15: return new List<int>(){12,20};
                case 16: return new List<int>(){13,17};
                case 17: return new List<int>(){16,18};
                case 18: return new List<int>(){14,17,19};
                case 19: return new List<int>(){18,20};
                case 20: return new List<int>(){15,19};
		        default:return new List<int>(){};
	        }
        }

        static int statesCount = 21;

        static int CountStates(List<Sequence> trainingSet)
        {
            return 21;
            
            List<int> allElements = new List<int>();
            foreach (Sequence item in trainingSet)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (!allElements.Contains(item[i]))
                        allElements.Add(item[i]);
                }
            }
            return allElements.Count;
        }

        static List<Triplet> GetTripletsFromSequence(Sequence s)
        {
            List<Triplet> results = new List<Triplet>();
            for (int i = 0; i < s.Count; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    Triplet t = new Triplet();
                    t.x0 = s[j-1];
                    t.x1 = s[j];
                    t.g = s[i];
                    results.Add(t);
                }
            }
            return results;
        }

        static List<Triplet> GetTripletsFromSequences(List<Sequence> ss, bool forceBidirectionality)
        {
            List<Triplet> results = new List<Triplet>();
            foreach (Sequence s in ss)
            {
                results.AddRange(GetTripletsFromSequence(s));
                if (forceBidirectionality)
                {
                    Sequence rs = new Sequence(s);
                    rs.Reverse();
                    results.AddRange(GetTripletsFromSequence(rs));
                }
            }
            return results;
        }

        static double[] OneHot(int i)
        {
            double[] code = new double[statesCount];
            for (int j = 0; j < statesCount; j++)
            {
                if (i == j)
                    code[j] = 1.0;
                else
                    code[j] = 0.0;

            }
            return code;
        }

        static void GetTrainingSet(List<Triplet> triplets, out double[][] input, out double[][] output)
        {
            input = new double[triplets.Count][];
            output = new double[triplets.Count][];

            for (int i = 0; i < triplets.Count; i++)
            {
                double[] x0 = OneHot(triplets[i].x0);
                double[] g = OneHot(triplets[i].g);
                input[i] = x0.Concat(g).ToArray();
                output[i] = OneHot(triplets[i].x1);
            }
        }
        static void GetTrainingSet(List<Triplet> triplets, out double[][] io)
        {
            io = new double[triplets.Count][];

            for (int i = 0; i < triplets.Count; i++)
            {
                double[] x0 = OneHot(triplets[i].x0);
                double[] g = OneHot(triplets[i].g);
                double[] x1 = OneHot(triplets[i].x1);
                io[i] = x0.Concat(g).ToArray().Concat(x1).ToArray();
            }
        }
        static bool FindPath(ActivationNetwork network, int start, int end, ref List<int> path, bool hidePrintout = false)
        {
            if (path.Count()>0 && path.Last() != start)
                path.Add(start);
            int current = start;
            int goal = end;

            while (current != goal)
            {
                double[] x0 = OneHot(current);
                double[] g = OneHot(goal);
                double[] input = x0.Concat(g).ToArray();
                double[] output = network.Compute(input);
                
                double maxValue = output.Max();
                int maxIndex = output.ToList().IndexOf(maxValue);

                if(maxIndex == current)
                {
                    if (!hidePrintout)
                        Console.WriteLine("Error: X0 == X1 ("+current+"-->"+maxIndex+")");
                    return false;
                } 
                if (path.Count(i => i == maxIndex) > 4)
                {
                    if (!hidePrintout)
                        Console.WriteLine("Infinite loop (" + current + "-->" + maxIndex + ")");
                    return false;
                }

                List<int> validMoves = ValidMoves(current);
                if (!validMoves.Contains(maxIndex))
                {
                    if (!hidePrintout)
                    {
                        Console.WriteLine("Error: Invalid Move (" + current + "-->" + maxIndex + ")");
                        Console.WriteLine("Try to find a partial path (" + current + "-->" + maxIndex + ")");
                    }
                    if (current == start && maxIndex == end)
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal infinite loop (" + current + "-->" + maxIndex + ")");
                        return false;
                    }
                    FindPath(network, current, maxIndex, ref path, hidePrintout);
                    if (path.Count()>0 && path.Last() == maxIndex)
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal success (" + current + "-->" + maxIndex + ")");
                    }
                    else
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal failed (" + current + "-->" + maxIndex + ")");
                        return false;
                    }
                }
                else
                {
                    path.Add(maxIndex);
                }

                current = maxIndex;
            }
            return true;
        }
        static bool FindPath(DistanceNetwork network, int start, int end, ref List<int> path, bool hidePrintout = false)
        {
            if (path.Count() > 0 && path.Last() != start)
                path.Add(start);
            int current = start;
            int goal = end;

            while (current != goal)
            {
                double[] x0 = OneHot(start);
                double[] g = OneHot(goal);
                double[] x1 = new double[statesCount]; //empty
                double[] input = x0.Concat(g).ToArray().Concat(x1).ToArray();
                double[] output = network.Compute(input);

                int maxIndex = statesCount * 2;
                for (int i = 0; i < statesCount; i++)
                {
                    if (output[statesCount * 2 + i] > output[maxIndex])
                        maxIndex = statesCount * 2 + i;
                }

                if (maxIndex == current)
                {
                    if (!hidePrintout)
                        Console.WriteLine("Error: X0 == X1 (" + current + "-->" + maxIndex + ")");
                    return false;
                }
                if (path.Count(i => i == maxIndex) > 4)
                {
                    if (!hidePrintout)
                        Console.WriteLine("Infinite loop (" + current + "-->" + maxIndex + ")");
                    return false;
                }

                List<int> validMoves = ValidMoves(current);
                if (!validMoves.Contains(maxIndex))
                {
                    if (!hidePrintout)
                    {
                        Console.WriteLine("Error: Invalid Move (" + current + "-->" + maxIndex + ")");
                        Console.WriteLine("Try to find a partial path (" + current + "-->" + maxIndex + ")");
                    }
                    if (current == start && maxIndex == end)
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal infinite loop (" + current + "-->" + maxIndex + ")");
                        return false;
                    }
                    FindPath(network, current, maxIndex, ref path, hidePrintout);
                    if (path.Count() > 0 && path.Last() == maxIndex)
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal success (" + current + "-->" + maxIndex + ")");
                    }
                    else
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Subgoal failed (" + current + "-->" + maxIndex + ")");
                        return false;
                    }
                }
                else
                {
                    path.Add(maxIndex);
                }

                current = maxIndex;
            }
            return true;
        }
        static void GenerateReport(List<Triplet> triplets, ActivationNetwork network)
        {
            foreach (Triplet t in triplets)
            {
                double[] x0 = OneHot(t.x0);
                double[] g = OneHot(t.g);
                double[] input = x0.Concat(g).ToArray();
                double[] output = network.Compute(input);
                double maxValue = output.Max();
                int maxIndex = output.ToList().IndexOf(maxValue);
                if (maxIndex != t.x1)
                {
                    Console.WriteLine("Post training error on " + t.ToString());
                    Console.WriteLine("Predicted " + maxIndex + "(" + maxValue.ToString("N2") + ")");
                }
            }
        }
        static void GenerateReport(List<Triplet> triplets, DistanceNetwork network)
        {
            foreach (Triplet t in triplets)
            {
                double[] x0 = OneHot(t.x0);
                double[] g = OneHot(t.g);
                double[] x1 = new double[statesCount]; //empty
                double[] input = x0.Concat(g).ToArray().Concat(x1).ToArray();
                double[] output = network.Compute(input);

                int maxIndex = statesCount * 2;
                for (int i = 0; i < statesCount; i++)
                {
                    if (output[statesCount * 2 + i] > output[maxIndex])
                        maxIndex = statesCount * 2 + i;
                }
                if (maxIndex != t.x1)
                {
                    Console.WriteLine("Post training error on " + t.ToString());
                    Console.WriteLine("Predicted " + maxIndex + "(" + output[maxIndex].ToString("N2") + ")");
                }
            }
        }
        static void ReportPath(ActivationNetwork network, int start, int end)
        {
            Console.WriteLine("From " + start + " to " + end);

            List<int> path = new List<int>();
            bool pathFound = FindPath(network, start, end, ref path);

            string pathStr = "Computed path = ";
            foreach (int step in path)
            {
                pathStr += step + " --> ";
            }
            Console.WriteLine(pathStr);
        }
        static void ReportPath(DistanceNetwork network, int start, int end)
        {
            Console.WriteLine("From " + start + " to " + end);

            List<int> path = new List<int>();
            bool pathFound = FindPath(network, start, end, ref path);

            string pathStr = "Computed path = ";
            foreach (int step in path)
            {
                pathStr += step + " --> ";
            }
            Console.WriteLine(pathStr);
        }

        static List<Sequence> GenerateRandomSequences(int count, int lenght)
        {
            Random rand = new Random();
            List<Sequence> seqs = new List<Sequence>();
            for (int i = 0; i < count; i++)
            {
                Sequence s = new Sequence();
                int start = rand.Next(statesCount);
                s.Add(start);
                while(s.Count <lenght)
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
        static void Main(string[] args)
        {
            List<Sequence> trainingSet = new List<Sequence>();

            List<Sequence> sequenceGoalDirected = new List<Sequence>() 
            { 
                new Sequence() { 2,6,10,14,18 },
                new Sequence() { 2,6,10,9,8 },
                new Sequence() { 2,6,10,11,12 }            
            };

            List<Sequence> sequenceShortcutsEasy = new List<Sequence>() 
            { 
                new Sequence() { 0,1,2,3,4 },
                new Sequence() { 4,7,12,15,20 },
                new Sequence() { 20,19,18,17,16},
                new Sequence() { 8,9,10,11,12 },   
                new Sequence() { 2,6,10,14,18 }           
            };

            List<Sequence> sequenceShortcuts = new List<Sequence>() 
            { 
                new Sequence() { 8,5,0,1,2,3,4,7,12  },
                new Sequence() { 0,5,8,13,16 },
                new Sequence() { 0,1,2,6,10,14,18,19,20 },
                new Sequence() { 8,9,10,11,12,15,20,19,18,17,16 }            
            };

            //List<Sequence> setToUse = sequenceGoalDirected;
            //List<Sequence> setToUse = sequenceShortcutsEasy;
            //List<Sequence> setToUse = sequenceShortcuts;
            List<Sequence> setToUse = GenerateRandomSequences(10, 7);

            //Prepare the data
            bool forceBidirectionality = true;
            statesCount = CountStates(setToUse);
            List<Triplet> triplets = GetTripletsFromSequences(setToUse, forceBidirectionality);
            foreach (Triplet item in triplets)
            {
                Console.WriteLine(item.ToString());
            }

            double[][] input;
            double[][] output;
            GetTrainingSet(triplets, out input, out output);
            double[][] io;
            GetTrainingSet(triplets, out io);

            bool useAutoAssociativeNetwork = false;

            if (!useAutoAssociativeNetwork)
            {
                //Create the network & train
                ActivationNetwork goalNetwork = goalNetwork = new ActivationNetwork(new SigmoidFunction(2.0), 2 * statesCount, 20, 10, statesCount);
                ParallelResilientBackpropagationLearning goalTeacher = new ParallelResilientBackpropagationLearning(goalNetwork);

                double error = double.PositiveInfinity;
                double stopError = 1.0;
                while (error > stopError)
                {
                    Console.WriteLine("Reset");
                    goalNetwork.Randomize();
                    //goalTeacher.Reset(0.0125);
                    for (int epoch = 0; epoch < 500 && error > stopError; epoch++)
                    {
                        error = goalTeacher.RunEpoch(input, output);
                        Console.WriteLine("Epoch " + epoch + " = \t" + error);
                    }
                }
                GenerateReport(triplets, goalNetwork);
                goalNetwork.Save("goalNetwork.mlp");
                //Test the network
                Console.WriteLine("------------------");
                Console.WriteLine("Finding paths...");

                Console.WriteLine("------------------");
                Console.WriteLine("TRAINED SEQUENCES");
                foreach (Sequence seq in setToUse)
                {
                    ReportPath(goalNetwork, seq.First(), seq.Last());
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                }
                Console.WriteLine("------------------");
                Console.WriteLine("RANDOM SEQUENCES");
                Random rand = new Random();

                int success = 0;
                int RANDOM_TRIALS = 100000;
                for (int i = 0; i < RANDOM_TRIALS; i++)
                {
                    List<int> path = new List<int>();
                    if (FindPath(goalNetwork, rand.Next(statesCount), rand.Next(statesCount), ref path, true))
                        success++;
                }
                Console.WriteLine("Success percentage over " + RANDOM_TRIALS + " = " + (100.0 * success / (double)RANDOM_TRIALS) + "%");

                while (true)
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                    ReportPath(goalNetwork, rand.Next(statesCount), rand.Next(statesCount));
                }
                Console.WriteLine("Finding paths, over.");
            }
            else //AUTO ASSOCIATIVE
            {
                DistanceNetwork goalNetworkAuto = new DistanceNetwork(3 * statesCount, 400);
                SOMLearning goalTeacherAuto = new SOMLearning(goalNetworkAuto);
                goalTeacherAuto.LearningRate = 0.3;
                goalTeacherAuto.LearningRadius = 5;
                double error = double.PositiveInfinity;
                double stopError = 0.01;
                while (error > stopError)
                {
                    Console.WriteLine("Reset");
                    goalNetworkAuto.Randomize();
                    //goalTeacher.Reset(0.0125);
                    for (int epoch = 0; epoch < 1500 && error > stopError; epoch++)
                    {
                        error = goalTeacherAuto.RunEpoch(io);
                        Console.WriteLine("Epoch " + epoch + " = \t" + error);
                    }
                }
                GenerateReport(triplets, goalNetworkAuto);

                //Test the network
                Console.WriteLine("------------------");
                Console.WriteLine("Finding paths...");

                Console.WriteLine("------------------");
                Console.WriteLine("TRAINED SEQUENCES");
                foreach (Sequence seq in setToUse)
                {
                    ReportPath(goalNetworkAuto, seq.First(), seq.Last());
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                }
                Console.WriteLine("------------------");
                Console.WriteLine("RANDOM SEQUENCES");
                Random rand = new Random();

                int success = 0;
                int RANDOM_TRIALS = 100000;
                for (int i = 0; i < RANDOM_TRIALS; i++)
                {
                    List<int> path = new List<int>();
                    if (FindPath(goalNetworkAuto, rand.Next(statesCount), rand.Next(statesCount), ref path, true))
                        success++;
                }
                Console.WriteLine("Success percentage over " + RANDOM_TRIALS + " = " + (100.0 * success / (double)RANDOM_TRIALS) + "%");

                while (true)
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                    ReportPath(goalNetworkAuto, rand.Next(statesCount), rand.Next(statesCount));
                }
                Console.WriteLine("Finding paths, over.");
            }
        }


        public static object DeepClone(object obj)
        {
            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }
    }
}
