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

using Accord.Math.Decompositions;
using Accord.Math;

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
        static double[] OneHot(int i, int statesCount)
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
        static void GetTrainingSet(Maze maze, List<Triplet> triplets, out double[][] input, out double[][] output)
        {
            input = new double[triplets.Count][];
            output = new double[triplets.Count][];

            for (int i = 0; i < triplets.Count; i++)
            {
                double[] x0 = OneHot(triplets[i].x0, maze.StatesCount);
                double[] g = OneHot(triplets[i].g, maze.StatesCount);
                input[i] = x0.Concat(g).ToArray();
                output[i] = OneHot(triplets[i].x1, maze.StatesCount);
            }
        }
        static void GetTrainingSet(Maze maze, List<Triplet> triplets, out double[][] io)
        {
            io = new double[triplets.Count][];

            for (int i = 0; i < triplets.Count; i++)
            {
                double[] x0 = OneHot(triplets[i].x0, maze.StatesCount);
                double[] g = OneHot(triplets[i].g, maze.StatesCount);
                double[] x1 = OneHot(triplets[i].x1, maze.StatesCount);
                io[i] = x0.Concat(g).ToArray().Concat(x1).ToArray();
            }
        }
        static bool FindPath(Maze maze, ActivationNetwork network, int start, int end, ref List<int> path, bool hidePrintout = false)
        {
            if (path.Count()>0 && path.Last() != start)
                path.Add(start);
            int current = start;
            int goal = end;

            while (current != goal)
            {
                double[] x0 = OneHot(current,maze.StatesCount);
                double[] g = OneHot(goal, maze.StatesCount);
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

                List<int> validMoves = maze.ValidMoves(current);
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
                    FindPath(maze, network, current, maxIndex, ref path, hidePrintout);
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
        static bool FindPath(Maze maze, DistanceNetwork network, int start, int end, ref List<int> path, bool hidePrintout = false)
        {
            if (path.Count() > 0 && path.Last() != start)
                path.Add(start);
            int current = start;
            int goal = end;

            while (current != goal)
            {
                double[] x0 = OneHot(start, maze.StatesCount);
                double[] g = OneHot(goal, maze.StatesCount);
                double[] x1 = new double[maze.StatesCount]; //empty
                double[] input = x0.Concat(g).ToArray().Concat(x1).ToArray();
                double[] output = network.Compute(input);

                int maxIndex = maze.StatesCount * 2;
                for (int i = 0; i < maze.StatesCount; i++)
                {
                    if (output[maze.StatesCount * 2 + i] > output[maxIndex])
                        maxIndex = maze.StatesCount * 2 + i;
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

                List<int> validMoves = maze.ValidMoves(current);
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
                    FindPath(maze, network, current, maxIndex, ref path, hidePrintout);
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
        static void GenerateReport(Maze maze, List<Triplet> triplets, ActivationNetwork network)
        {
            foreach (Triplet t in triplets)
            {
                double[] x0 = OneHot(t.x0,maze.StatesCount);
                double[] g = OneHot(t.g, maze.StatesCount);
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
        static void GenerateReport(Maze maze, List<Triplet> triplets, DistanceNetwork network)
        {
            foreach (Triplet t in triplets)
            {
                double[] x0 = OneHot(t.x0, maze.StatesCount);
                double[] g = OneHot(t.g, maze.StatesCount);
                double[] x1 = new double[maze.StatesCount]; //empty
                double[] input = x0.Concat(g).ToArray().Concat(x1).ToArray();
                double[] output = network.Compute(input);

                int maxIndex = maze.StatesCount * 2;
                for (int i = 0; i < maze.StatesCount; i++)
                {
                    if (output[maze.StatesCount * 2 + i] > output[maxIndex])
                        maxIndex = maze.StatesCount * 2 + i;
                }
                if (maxIndex != t.x1)
                {
                    Console.WriteLine("Post training error on " + t.ToString());
                    Console.WriteLine("Predicted " + maxIndex + "(" + output[maxIndex].ToString("N2") + ")");
                }
            }
        }
        static void ReportPath(Maze maze, ActivationNetwork network, int start, int end)
        {
            Console.WriteLine("From " + start + " to " + end);

            List<int> path = new List<int>();
            bool pathFound = FindPath(maze, network, start, end, ref path);

            string pathStr = "Computed path = ";
            foreach (int step in path)
            {
                pathStr += step + " --> ";
            }
            Console.WriteLine(pathStr);
        }
        static void ReportPath(Maze maze, DistanceNetwork network, int start, int end)
        {
            Console.WriteLine("From " + start + " to " + end);

            List<int> path = new List<int>();
            bool pathFound = FindPath(maze, network, start, end, ref path);

            string pathStr = "Computed path = ";
            foreach (int step in path)
            {
                pathStr += step + " --> ";
            }
            Console.WriteLine(pathStr);
        }

        static void ChooseRandomPair(out int start, out int end, Maze maze, List<Triplet> trainingSet, bool canBeSamePoint, bool canBePartOfTrainingSet)
        {
            Random rand = new Random();

            start = rand.Next(maze.StatesCount);
            end = rand.Next(maze.StatesCount);

            if (!canBeSamePoint)
            {
                while (start == end)
                    end = rand.Next(maze.StatesCount);
            }

            if (!canBePartOfTrainingSet)
            {
                bool found = true;
                while (found)
                {
                    found = false;
                    start = rand.Next(maze.StatesCount);
                    end = rand.Next(maze.StatesCount);
                    foreach (Triplet t in trainingSet)
                    {
                        found = (t.x0 == start && t.g == end);
                        if (found)
                            break;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Maze maze;
            List<Sequence> setToUse;
            Generate_21(out maze, out setToUse);
            //Generate_T(out maze, out setToUse);

            //Prepare the data
            bool forceBidirectionality = true;

            List<Triplet> triplets = GetTripletsFromSequences(setToUse, forceBidirectionality);
            foreach (Triplet item in triplets)
            {
                Console.WriteLine(item.ToString());
            }

            double[][] input;
            double[][] output;
            GetTrainingSet(maze, triplets, out input, out output);
            double[][] io;
            GetTrainingSet(maze, triplets, out io);

            bool useAutoAssociativeNetwork = false;

            if (!useAutoAssociativeNetwork)
            {
                //Create the network & train
                ActivationNetwork goalNetwork = goalNetwork = new ActivationNetwork(new SigmoidFunction(2.0), 2 * maze.StatesCount, 20, maze.StatesCount);
                ParallelResilientBackpropagationLearning goalTeacher = new ParallelResilientBackpropagationLearning(goalNetwork);

                double error = double.PositiveInfinity;
                double stopError = 10.0;
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
                GenerateReport(maze, triplets, goalNetwork);
                goalNetwork.Save("goalNetwork.mlp");
                //Test the network
                Console.WriteLine("------------------");
                Console.WriteLine("Finding paths...");

                Console.WriteLine("------------------");
                Console.WriteLine("TRAINED SEQUENCES");
                foreach (Sequence seq in setToUse)
                {
                    ReportPath(maze, goalNetwork, seq.First(), seq.Last());
                    //Console.WriteLine("Press a key to continue...");
                    //Console.ReadKey();
                }
                Console.WriteLine("------------------");
                Console.WriteLine("RANDOM SEQUENCES");
                Random rand = new Random();

                int success = 0;
                int RANDOM_TRIALS = 100000;
                for (int i = 0; i < RANDOM_TRIALS; i++)
                {
                    List<int> path = new List<int>();
                    int startIndex,endIndex;
                    ChooseRandomPair(out startIndex, out endIndex, maze, triplets, false, false);
                    if (FindPath(maze, goalNetwork, startIndex, endIndex, ref path, true))
                        success++;
                }
                Console.WriteLine("Success percentage over " + RANDOM_TRIALS + " = " + (100.0 * success / (double)RANDOM_TRIALS) + "%");

                while (true)
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                    ReportPath(maze, goalNetwork, rand.Next(maze.StatesCount), rand.Next(maze.StatesCount));
                }
                Console.WriteLine("Finding paths, over.");
            }
            else //AUTO ASSOCIATIVE
            {
                DistanceNetwork goalNetworkAuto = new DistanceNetwork(3 * maze.StatesCount, 400);
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
                GenerateReport(maze, triplets, goalNetworkAuto);

                //Test the network
                Console.WriteLine("------------------");
                Console.WriteLine("Finding paths...");

                Console.WriteLine("------------------");
                Console.WriteLine("TRAINED SEQUENCES");
                foreach (Sequence seq in setToUse)
                {
                    ReportPath(maze, goalNetworkAuto, seq.First(), seq.Last());
                    //Console.WriteLine("Press a key to continue...");
                    //Console.ReadKey();
                }
                Console.WriteLine("------------------");
                Console.WriteLine("RANDOM SEQUENCES");
                Random rand = new Random();

                int success = 0;
                int RANDOM_TRIALS = 100000;
                for (int i = 0; i < RANDOM_TRIALS; i++)
                {
                    List<int> path = new List<int>();
                    int startIndex, endIndex;
                    ChooseRandomPair(out startIndex, out endIndex, maze, triplets, false, false);
                    if (FindPath(maze, goalNetworkAuto, startIndex, endIndex, ref path, true))
                        success++;
                }
                Console.WriteLine("Success percentage over " + RANDOM_TRIALS + " = " + (100.0 * success / (double)RANDOM_TRIALS) + "%");

                while (true)
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                    ReportPath(maze, goalNetworkAuto, rand.Next(maze.StatesCount), rand.Next(maze.StatesCount));
                }
                Console.WriteLine("Finding paths, over.");
            }
        }

        static void Generate_21(out Maze maze, out List<Sequence> trainingSet)
        {
            maze = new Maze
            (
                new double[,]
                {
                    //0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20
                    {0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//0
                    {1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//1
                    {0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//2
                    {0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//3
                    {0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//4
                    {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//5
                    {0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//6
                    {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},//7
                    {0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},//8
                    {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//9

                    {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0},//10
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},//11
                    {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0},//12
                    {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},//13
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},//14
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1},//15
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0},//16
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0},//17
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0},//18
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1},//19
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0},//20
                }
            );

            //Training set
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

            List<Sequence> rndSequence = maze.GenerateRandomSequences(10,2);

            trainingSet = sequenceShortcuts;// sequenceGoalDirected;
        }

        static void Generate_T(out Maze maze, out List<Sequence> trainingSet)
        {
            maze = new Maze
            (
                new double[,]
                {
                    //0 1  2  3  4  5  6
                    {0, 1, 0, 0, 0, 0, 0},//0
                    {1, 0, 1, 0, 0, 0, 0},//1
                    {0, 1, 0, 1, 0, 1, 0},//2
                    {0, 0, 1, 0, 1, 0, 0},//3
                    {0, 0, 0, 1, 0, 0, 0},//4
                    {0, 0, 1, 0, 0, 0, 1},//5
                    {0, 0, 0, 0, 0, 1, 0},//6
                }
            );

            //Training set
            List<Sequence> sequence_length1_full_coverage = new List<Sequence>() 
            { 
                new Sequence() { 0,1 },
                new Sequence() { 1,2 },
                new Sequence() { 2,3 },
                new Sequence() { 3,4 },
                new Sequence() { 2,5 },
                new Sequence() { 5,6 },           
            };

            List<Sequence> sequence_length2_full_coverage = new List<Sequence>() 
            { 
                new Sequence() { 0,1,2 },
                new Sequence() { 2,3,4 },
                new Sequence() { 4,5,6 }            
            };

            List<Sequence> sequence_fullCoverage_overlap = new List<Sequence>() 
            { 
                new Sequence() { 0,1,2,3,4 }, 
                new Sequence() { 4,3,2,5,6 },           
            };


            List<Sequence> rndSequence = maze.GenerateRandomSequences(10, 2);

            trainingSet = sequence_fullCoverage_overlap;// sequence_length2_full_coverage;
        }
    }
}
