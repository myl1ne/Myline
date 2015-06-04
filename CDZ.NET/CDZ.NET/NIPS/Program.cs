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
    #region Datastructure
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

        override public String ToString()
        {
            return "G=" + g + "\tX0=" + x0 + "\tX1=" + x1;
        }
    }
    #endregion Datastructure

    class Program
    {
        const int MAX_RECURSIVE_DEPTH = 7;

        static void Main(string[] args)
        {
            int VERBOSITY = 0;
            StreamWriter logFile = new StreamWriter("logFileParallel.csv");

            //---------------------------------------------  CREATE MAZE ---------------------------------------------
            Maze maze;
            Dictionary<string, List<Sequence> > setsToUse;
            Generate_21(out maze, out setsToUse);
            string mazeType = "mat_21";
            //Generate_T(out maze, out setsToUse);
            //string mazeType = "mat_T";

            //double randomPathQuality = 0.0;
            //ComputePathQualityRandom(maze, out randomPathQuality, 1000);
            //Console.WriteLine("Random Path Quality = " + randomPathQuality.ToString("N2"));

            logFile.WriteLine("maze,trainingSet,trainSetSize,testSet,performance,lengthPerformance,trainingError");

            for (int run = 0; run < 10; run++)
            {
                Console.WriteLine("RUN NUMBER "+run);

                //Loop through all the training sets
                foreach (string trainSetName in setsToUse.Keys)
                {
                    List<Sequence> setToUse = setsToUse[trainSetName];
                    Console.WriteLine("\n---------------------------------------------------------");
                    Console.WriteLine("TRAINING SET : " + trainSetName);

                    //---------------------------------------------  PREPARE DATA ---------------------------------------------
                    bool forceBidirectionality = true;

                    List<Triplet> triplets = GetTripletsFromSequences(setToUse, forceBidirectionality);
                    if (VERBOSITY > 1)
                    {
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Training set elements");
                        foreach (Triplet item in triplets)
                        {
                            Console.WriteLine(item.ToString());
                        }
                        Console.WriteLine("---------------------------------------------------------");
                    }

                    double[][] input;
                    double[][] output;
                    GetTrainingSet(maze, triplets, out input, out output);

                    //FOR AUTOASSOCIATION
                    //double[][] io;
                    //GetTrainingSet(maze, triplets, out io);

                    //---------------------------------------------  CREATE NETWORK ---------------------------------------------
                    //Create the network & train
                    //var function = new BipolarSigmoidFunction();
                    var function = new SigmoidFunction(2.0);
                    ActivationNetwork goalNetwork = goalNetwork = new ActivationNetwork(function, 2 * maze.StatesCount, 20, maze.StatesCount);
                    ParallelResilientBackpropagationLearning goalTeacher = new ParallelResilientBackpropagationLearning(goalNetwork);
                    //BackPropagationLearning goalTeacher = new BackPropagationLearning(goalNetwork);

                    int epoch = 0;
                    double stopError = 0.1;
                    int resets = 0;
                    double minimumErrorReached = double.PositiveInfinity;
                    while (minimumErrorReached > stopError && resets < 1)
                    {
                        goalNetwork.Randomize();
                        goalTeacher.Reset(0.0125);

                        double error = double.PositiveInfinity;
                        for (epoch = 0; epoch < 500 && error > stopError; epoch++)
                        {
                            error = goalTeacher.RunEpoch(input, output);
                            //Console.WriteLine("Epoch " + epoch + " = \t" + error);

                            if (error < minimumErrorReached)
                            {
                                minimumErrorReached = error;
                                goalNetwork.Save("goalNetwork.mlp");
                            }
                        }
                        //Console.Write("Reset (" + error+")->");
                        Console.Write(".(" + error.ToString("N2") + ") ");
                        resets++;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Best error obtained =" + minimumErrorReached);

                    goalNetwork = ActivationNetwork.Load("goalNetwork.mlp") as ActivationNetwork;

                    if (VERBOSITY > 0)
                        GenerateReport(maze, triplets, goalNetwork);

                    //---------------------------------------------  TEST ---------------------------------------------

                    Console.WriteLine("------------------");
                    Console.WriteLine("Finding paths...");
                    double score, lengthScore;
                    double[,] pathMatrix;
                    ComputePathMatrix(maze, goalNetwork, out score, out lengthScore, out pathMatrix, trainSetName);

                    Console.WriteLine("Success over whole input space = " + score.ToString("N2") + "% and lengthScore=" + lengthScore.ToString("N2"));
                    logFile.WriteLine(mazeType + "," + trainSetName + "," + triplets.Count + "," + "whole-input-space" + "," + score + "," + lengthScore + "," + minimumErrorReached);
 
                    List<Triplet> setToEvaluate = triplets;
                    EvaluateSpecificSet(maze, pathMatrix, setToEvaluate, out score, out lengthScore);
                    Console.WriteLine("Success percentage over training set = " + score.ToString("N2") + "% and lengthScore=" + lengthScore.ToString("N2"));
                    logFile.WriteLine(mazeType + "," + trainSetName + "," + triplets.Count + "," + "training-set" + "," + score + "," + lengthScore + "," + minimumErrorReached);
                    
                    setToEvaluate = GenerateTestSet_1LengthPath(maze);
                    EvaluateSpecificSet(maze, pathMatrix, setToEvaluate, out score, out lengthScore);
                    Console.WriteLine("Success percentage over 1-length set = " + score.ToString("N2") + "% and lengthScore=" + lengthScore.ToString("N2"));
                    logFile.WriteLine(mazeType + "," + trainSetName + "," + triplets.Count + "," + "length-1-sequences" + "," + score + "," + lengthScore + "," + minimumErrorReached);
                    
                    logFile.Flush();
                    Console.WriteLine("Finding paths, over.");
                    //Console.ReadKey();
                }
            }
            logFile.Close();
            Console.ReadKey();
        }


        #region FUNCTIONS

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
        static bool FindPath(Maze maze, ActivationNetwork network, int start, int end, ref List<int> path, ref int recursiveDepth,ref int recursionLevelReached, bool hidePrintout = false )
        {
            if (path.Count == 0 || (path.Count() > 0 && path.Last() != start))
                path.Add(start);
            int current = start;
            int goal = end;
            recursionLevelReached = Math.Max(recursionLevelReached, recursiveDepth);

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
                    if (recursiveDepth < MAX_RECURSIVE_DEPTH)
                    {
                        recursiveDepth++;
                        FindPath(maze, network, current, maxIndex, ref path, ref recursiveDepth, ref recursionLevelReached, hidePrintout);
                        recursiveDepth--;
                    }
                    else
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Max depth level reached. Cancelled.");
                        return false;
                    }
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
        static bool FindPath(Maze maze, DistanceNetwork network, int start, int end, ref List<int> path,ref int recursiveDepth, bool hidePrintout = false)
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
                    if (recursiveDepth < MAX_RECURSIVE_DEPTH)
                    {
                        recursiveDepth++;
                        FindPath(maze, network, current, maxIndex, ref path, ref recursiveDepth, hidePrintout);
                        recursiveDepth--;
                    }
                    else
                    {
                        if (!hidePrintout)
                            Console.WriteLine("Max depth level reached. Cancelled.");
                        return false;
                    }
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
            int rd = 0;
            int rdreached = 0;
            bool pathFound = FindPath(maze, network, start, end, ref path, ref rd, ref rdreached);

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
            int rd = 0;
            bool pathFound = FindPath(maze, network, start, end, ref path, ref rd);

            string pathStr = "Computed path = ";
            foreach (int step in path)
            {
                pathStr += step + " --> ";
            }
            Console.WriteLine(pathStr);
        }

        static void ChooseRandomPair(out int start, out int end, Maze maze, List<Triplet> trainingSet, bool canBePartOfTrainingSet)
        {
            Random rand = new Random();
            start = rand.Next(maze.StatesCount);
            end = rand.Next(maze.StatesCount);

            if (!canBePartOfTrainingSet)
            {
                bool found = true;
                while (found)
                {
                    found = false;
                    start = rand.Next(maze.StatesCount);
                    end = rand.Next(maze.StatesCount);
                    while(start==end)
                    {
                        start = rand.Next(maze.StatesCount);
                        end = rand.Next(maze.StatesCount);
                    }
                    foreach (Triplet t in trainingSet)
                    {
                        found = (t.x0 == start && t.g == end);
                        if (found)
                            break;
                    }
                }
            }
            else
            {
                start = rand.Next(maze.StatesCount);
                end = rand.Next(maze.StatesCount);
                while (start == end)
                {
                    start = rand.Next(maze.StatesCount);
                    end = rand.Next(maze.StatesCount);
                }
            }
        }
        #endregion FUNCTIONS

        #region MAZES
        static void Generate_21(out Maze maze, out Dictionary<string, List<Sequence>> trainingSets)
        {
            Console.WriteLine("Using Maze-21");
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

            //List<Sequence> rndSequence10_5 = maze.GenerateRandomSequences(10, 5);
            //List<Sequence> rndSequence10_10 = maze.GenerateRandomSequences(10, 10);

            trainingSets = new Dictionary<string, List<Sequence>>();
            //trainingSets["sequenceGoalDirected"] = sequenceGoalDirected;
            //trainingSets["whole-coverage-no-overlap"] = sequenceShortcutsEasy;
            //trainingSets["whole-coverage-overlap"] = sequenceShortcuts;
            //trainingSets["sequence_random_count=10_length=5"] = rndSequence10_5;
            //trainingSets["sequence_random_count=10_length=10"] = rndSequence10_10;

            //for (int seqCount = 5; seqCount <= 20; seqCount += 5)
            //{
            //    for (int seqLength = 2; seqLength <= 10; seqLength += 1)
            //    {
            //        trainingSets["sequence_random_count=" + seqCount + "_length=" + seqLength] = maze.GenerateRandomSequences(seqCount, seqLength);
            //    }
            //}

            //Generate full covering random sequences
            for (int seeds = 3; seeds <= 10; seeds += 1)
            {
                trainingSets["rnd_full_coverage_no_overlap=" + seeds] = maze.GenerateFullCoverage(seeds,false);
                trainingSets["rnd_full_coverage_overlap=" + seeds] = maze.GenerateFullCoverage(seeds, true);
            }
        }

        static void Generate_T(out Maze maze, out Dictionary<string, List<Sequence> > trainingSets)
        {
            Console.WriteLine("Using Maze-T");
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

            trainingSets = new Dictionary<string, List<Sequence>>();
            trainingSets["sequence_length1_full_coverage"] = sequence_length1_full_coverage;
            trainingSets["sequence_length2_full_coverage"] = sequence_length2_full_coverage;
            trainingSets["sequence_fullCoverage_overlap"] = sequence_fullCoverage_overlap;
        }

        #endregion MAZES
        
        #region TEST SETS
        static List<Triplet> GenerateTestSet_1LengthPath(Maze maze)
        {
            List<Triplet> testSet = new List<Triplet>();
            for (int i = 0; i < maze.StatesCount; i++)
            {
                for (int j = 0; j < maze.StatesCount; j++)
                {
                    if (maze.AdjMatrix[i,j] != 0.0)
                    {
                        Triplet t = new Triplet();
                        t.x0 = i;
                        t.x1 = j;
                        t.g = j;
                        testSet.Add(t);
                    }
                }                
            }
            return testSet;
        }
        static void ComputePathQualityRandom(Maze m, out double avgLengthScore, int trials)
        {
            StreamWriter log = new StreamWriter("logRandomWalk.csv");
            log.WriteLine("start,end,rndLength,optimalLength");

            Random rand = new Random();
            avgLengthScore = 0.0;
            int pathFoundCount = 0;
            for (int i = 0; i < m.StatesCount; i++)
            {
                for (int j = 0; j < m.StatesCount; j++)
                {
                    if (i != j)
                    {
                        for (int trial = 0; trial < trials; trial++)
                        {
                            List<int> path = new List<int>();
                            int currentState = i;
                            bool pathFound = false;
                            while (!pathFound)
                            {
                                bool impass = false;
                                while (currentState != j && !impass)
                                {
                                    List<int> vMoves = m.ValidMoves(currentState);
                                    while (vMoves.Count>0 && path.Contains(currentState = vMoves[rand.Next(vMoves.Count)]))
                                        vMoves.Remove(currentState);
                                    if (vMoves.Count == 0)
                                        impass = true;
                                    else
                                        path.Add(currentState);
                                }
                                pathFound = !impass;
                            }

                            pathFoundCount++;
                            avgLengthScore += path.Count / m.DistMatrix[i, j];
                            log.WriteLine(i+","+j+","+path.Count+","+m.DistMatrix[i, j]);
                        }
                    }
                }
            }
            //Path found among all possible path (exclude diagonal)
            avgLengthScore /= (double)pathFoundCount;
            log.Close();
        }

        static void ComputePathMatrix(Maze m, ActivationNetwork net, out double performance, out double avgLengthScore, out double[,] pathMatrix, string trainingSetTag)
        {
            bool shouldWriteHeaders = !File.Exists("logPathMatrix.csv");
            StreamWriter log = new StreamWriter("logPathMatrix.csv",true);
            if (shouldWriteHeaders)
                log.WriteLine("trainingset,start,end,path,pathLength,optimalLength,recursionLevel");

            pathMatrix = new double[m.StatesCount, m.StatesCount];
            int pathFoundCount = 0;
            avgLengthScore = 0.0;
            for (int i = 0; i < m.StatesCount; i++)
            {
                for (int j = 0; j < m.StatesCount; j++)
                {
                    if (i == j)
                    {
                        pathMatrix[i, j] = 0.0;
                    }
                    else
                    {
                        List<int> path = new List<int>();
                        int rd = 0;
                        int rdreached = 0;
                        if (FindPath(m, net, i, j, ref path, ref rd, ref rdreached, true))
                        {
                            pathMatrix[i, j] = path.Count -1;
                            avgLengthScore += pathMatrix[i, j] / m.DistMatrix[i, j];
                            pathFoundCount++;
                        }
                        else
                        {
                            pathMatrix[i, j] = double.PositiveInfinity;
                        }


                        string pathStr = "";
                        foreach (int item in path)
	                    {
		                    pathStr += item+"->";
	                    }
                        log.WriteLine(trainingSetTag + "," + i + "," + j + "," + pathStr + "," + pathMatrix[i, j] + "," + m.DistMatrix[i, j] + "," + rdreached);
                    }

                    //if (double.IsPositiveInfinity(pathMatrix[i, j]))
                    //    Console.Write("." + "\t");
                    //else
                    //    Console.Write(pathMatrix[i, j].ToString("N0") + "\t");
                }
                //Console.WriteLine();
            }

            //Path found among all possible path (exclude diagonal)
            performance = 100.0* pathFoundCount / ((double)m.StatesCount * m.StatesCount - m.StatesCount);
            avgLengthScore /= pathFoundCount;
            log.Close();
        }

        static void EvaluateSpecificSet(Maze maze, double[,] pathMatrix, List<Triplet> setToEvaluate, out double score, out double lengthScore)
        {
            score = 0.0;
            lengthScore = 0.0;
            int totalElementsNonID = 0;
            int totalElementsID = 0;
            int totalElements = 0;
            foreach (Triplet item in setToEvaluate)
            {
                if (item.x0 == item.g)
                {
                    totalElementsID++;
                }
                else
                {
                    totalElements++;
                    if (!double.IsPositiveInfinity(pathMatrix[item.x0, item.g]))
                    {
                        totalElementsNonID++;
                        score += 1.0;
                        lengthScore += (pathMatrix[item.x0, item.g] / (double)maze.DistMatrix[item.x0, item.g]);
                    }
                }
            }
            if (totalElementsID>0)
            {
                Console.WriteLine("Warning: this set contained "+totalElementsID+" identical elements (goal == start)");
            }
            score = 100.0 * score / (double)totalElements;
            lengthScore = lengthScore / (double)totalElementsNonID;
        }
        #endregion TEST SETS

    }
}
