using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    class Program
    {
        class Sequence : List<double[]>
        {
            public string ToReadable()
            {
                string str = "";
                for(int i=0; i<Count;i++)
                {
                    str += "Elt " + i + "-->\t";
                    for (int j = 0; j < this[i].Length; j++)
                    {
                        str += this[i][j] + "\t";
                    }
                    str += "\n";
                }
                return str;
            }
        }

        static void Main(string[] args)
        {
            Dictionary<char, double[]> c;
            Dictionary< double[], char > c2;
            formLetters(out c, out c2);
            //double[] a = { 1, 0, 0, 0, 0 };
            //double[] b = { 0, 1, 0, 0, 0 };
            //double[] c = { 0, 0, 1, 0, 0 };
            //...
            List<Sequence> sequenceTestground = new List<Sequence>() 
            { 
                new Sequence() { c['a'], c['b'] },         
                new Sequence() { c['b'], c['c'] },         
                new Sequence() { c['c'], c['d'] },           
            };

            List<Sequence> sequenceGoalDirected = new List<Sequence>() 
            { 
                new Sequence() { c['c'], c['g'], c['k'], c['j'], c['i']  },
                new Sequence() { c['c'], c['g'], c['k'], c['o'], c['s'] },
                new Sequence() { c['c'], c['g'], c['k'], c['l'], c['m'] }            
            };

            List<Sequence> sequenceShortcuts = new List<Sequence>() 
            { 
                new Sequence() { c['a'], c['b'], c['c'], c['g'], c['k'], c['o'], c['s'], c['t'], c['u']  },
                new Sequence() { c['i'], c['j'], c['k'], c['l'], c['m'], c['p'], c['u'], c['t'], c['s'], c['r'], c['q']  },
                new Sequence() { c['a'], c['f'], c['i'], c['n'], c['q'] },
                new Sequence() { c['m'], c['h'], c['e'], c['d'], c['c'], c['b'], c['a'], c['f'], c['i'] }            
            };

            List<Sequence> setToUse = sequenceTestground;

            TimeCanvas canvas = new TimeCanvas(25,7);
            for (int i = 0; i < 5; i++)
            {
                foreach (Sequence s in setToUse)
                {
                    bool training = true;
                    string errorMsg = "\n\n ---------------------------------------- \n Sequence \n";
                    double seqMeanError = 0.0;

                    canvas.Reset();
                    foreach (double[] item in s)
                    {
                        if (item != s.First())
                        {
                            List<KeyValuePair<double[], double> > predictions = canvas.PredictAll();
                            double[] reality = item;
                            double itemError = CDZNET.MathHelpers.distance(predictions.First().Key, reality);
                            seqMeanError += itemError;
                            errorMsg += "Reality \t" + c2[reality] + "\t";
                            //errorMsg += "Predict \t"  + Convert(prediction, c2) + "\t";
                            errorMsg += "Predict \t";
                            foreach(KeyValuePair<double[], double> pre in predictions)
                            {
                                errorMsg += Convert(pre.Key, c2) + "(" + pre.Value + ")" + "\t";
                            }
                            errorMsg += "\nError   \t " + itemError + "\n";
                        }
                        if (training)
                            canvas.Train(item);
                    }
                    Console.WriteLine(errorMsg);
                }
            }
            canvas.Save("debug.csv");
            Console.WriteLine("Training over.");

            //Test the pathfinding
            Console.WriteLine("\n------------------\nFinding paths...");
            foreach (Sequence seq in setToUse)
            {
                Console.WriteLine("From " + Convert(seq.First(), c2) + " to " + Convert(seq.Last(), c2));
                List<TimeLine> path;
                bool pathFound = canvas.findPath(seq.First(), seq.Last(), out path);
                //canvas.findPath(c['a'], c['d'], out path);
                string pathStr = "Path = ";
                foreach(TimeLine pathElement in path)
                {
                    pathStr += Convert(pathElement.receptiveField, c2);
                    if (pathElement != path.Last())
                        pathStr += "->";
                }
                Console.WriteLine(pathStr);
            } 

            Console.WriteLine("Finding paths, over.");
        }

        static void formLetters(out Dictionary< char, double[] > char2code, out Dictionary< double[], char > code2char)
        {
            char2code = new Dictionary<char, double[]>();
            code2char = new Dictionary<double[], char>();

            for(char c = 'a'; c<'z';c++)
            {
                double[] code = new double[25];
                for(int i=0;i<25;i++)
                {
                    if (i == (c - 'a'))
                        code[i] = 1;
                    else
                        code[i] = 0;
                }
                char2code[c] = code;
                code2char[code] = c;
            }
        }
        static char Convert(double[] d, Dictionary<double[], char> code2char)
        {
            //find the closest
            foreach(double[] real in code2char.Keys)
            {
                if (real.SequenceEqual(d))
                    return code2char[real];
            }

            return '#';      
        }

        static string Convert(double[] d)
        {
            string str = "";
            for (int i = 0; i < d.Length; i++)
            {
                str += d[i] + "\t";   
            }
            return str;
        }
    }
}
