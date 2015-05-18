using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TimeCells;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace KaggleTaxis
{
    class Program
    {

        static void Main(string[] args)
        {            
            //----------------------------------------------------------------------------------------------
            //Create the network
            int timelineSize = 50;
            TimeCells.TimeCanvas canvas = new TimeCanvas(2, timelineSize, null);
            //TimeCells.TimeCanvas canvas = new TimeCanvas(2, 50, HarvesineDistance);
            canvas.parameterLineCreationTreshold = 0.01;
            canvas.parameterFFLearningRate = 0;

            bool doTraining = false;
            double avgTrajectoryLength = timelineSize;
            //----------------------------------------------------------------------------------------------
            //Load the training set
            if (doTraining)
            {
                double[] minBound = new double[2], maxBound = new double[2];
                //List<TaxiCourse> trainSet = LoadDataSet("C:\\Users\\Stephane\\Documents\\GitHub\\Myline\\Datasets\\Kaggle Taxis\\train.csv", ref minBound, ref maxBound, ref avgTrajectoryLength, -1);
                List<TaxiCourse> trainSet = LoadDataSet("D:\\robotology\\src\\Myline\\CDZ.NET\\Datasets\\Kaggle Taxis\\train.csv", ref minBound, ref maxBound, ref avgTrajectoryLength, -1);
                Console.WriteLine("Range is: Lattitude=" + (maxBound[0] - minBound[0]) + "\t Longitude=" + (maxBound[1] - minBound[1]));
                Console.WriteLine("MinMax are: Lattitude=" + maxBound[0] + "/" + minBound[0] + "\t Longitude=" + maxBound[1] + "/" + minBound[1]);


                //List<TaxiCourse> testSet = LoadDataSet("C:\\Users\\Stephane\\Documents\\GitHub\\Myline\\Datasets\\Kaggle Taxis\\test.csv", ref minBoundTest, ref maxBoundTest, ref avgTrajectoryLengthTest, -1);

                StreamWriter stats = new StreamWriter("stats.txt");
                stats.WriteLine("Min " + minBound[0] + " " + minBound[1]);
                stats.WriteLine("Max " + maxBound[0] + " " + maxBound[1]);
                stats.WriteLine("Lines " + trainSet.Count);
                stats.Close();

                //----------------------------------------------------------------------------------------------
                //Prepare the data
                //ScaleTrajectoryData(trainSet, minBound, maxBound);
                //ScaleTrajectoryData(testSet, minBound, maxBound);
                AddTerminalSymbol(trainSet, new double[] { 999, 999 });

                //Try to imprint (fast training)
                int imprintIteration = 0;

                Stopwatch watch2 = new Stopwatch();
                watch2.Start();
                foreach (TaxiCourse course in trainSet)
                {
                    canvas.Imprint(course.polyline);
                    imprintIteration++;
                    if (imprintIteration % 1000 == 0)
                    {
                        Console.WriteLine("Epoch done : " + Math.Floor(100 * imprintIteration / (double)trainSet.Count) + "%");
                    }
                }
                watch2.Stop();
                Console.WriteLine("Training set (" + trainSet.Count + " elements) processed in " + watch2.Elapsed + ".");
                canvas.Save("SavedCanvas.csv");
            }
            else //if (false) 
            {
                Console.WriteLine("Loading previously learnt canvas.");
                canvas.Load("SavedCanvas.csv");
                Console.WriteLine("Loading complete.");
            }

            //----------------------------------------------------------------------------------------------
            //Load the test set
            double[] minBoundTest = null, maxBoundTest = null;
            double avgTrajectoryLengthTest = 0.0;
            List<TaxiCourse> testSet = LoadDataSet("D:\\robotology\\src\\Myline\\CDZ.NET\\Datasets\\Kaggle Taxis\\test.csv", ref minBoundTest, ref maxBoundTest, ref avgTrajectoryLengthTest, -1);

            //----------------------------------------------------------------------------------------------
            //Generate the test results
            StreamWriter results = new StreamWriter("Results.csv");
            results.WriteLine("\"TRIP_ID\",\"LATITUDE\",\"LONGITUDE\"");
            foreach (TaxiCourse course in testSet)
            {
                canvas.Reset();

                //Load the begining of the sequence
                foreach(double[] point in course.polyline)
                {
                    canvas.PresentInput(point, false, false);
                    canvas.PropagateActivity();
                }

                double[] prediction = new double[] { 0, 0 };
                double[] lastPrediction = new double[] { 0, 0 };
                Console.WriteLine("Predicting"); 
                int iterationCount = 0;
                while (prediction[0] != 999 && prediction[1] != 999 && iterationCount < 2 * avgTrajectoryLength)
                {
                    prediction.CopyTo(lastPrediction,0);
                    canvas.PresentInput(prediction, false, false);
                    canvas.PropagateActivity();
                    //We take the best prediction after the actual spot
                    List<KeyValuePair<double[], double>> sortedPredictions = canvas.PredictAll();                        
                    foreach (KeyValuePair<double[], double> pair in sortedPredictions)
                    {
                        if (pair.Key[0] != prediction[0] || pair.Key[1] != prediction[1] )
                        {
                            pair.Key.CopyTo(prediction,0);
                            break;
                        }
                    }
                    Console.WriteLine(prediction[0] + "\t\t" + prediction[1]); 
                    iterationCount++;      
                }
                results.WriteLine("\""+course.tripID + "\"," + lastPrediction[1] + "," + lastPrediction[0]);
            }
            results.Close();
        }

        static double HarvesineDistance(double[] a, double[] b)
        {
            double alpha = Math.Pow(Math.Sin(Math.Abs(a[0] - b[0]) * Math.PI / 360.0), 2.0) + Math.Cos(a[0]) * Math.Cos(b[0]) * Math.Pow(Math.Sin(Math.Abs(a[1] - b[1]) * Math.PI / 360.0), 2.0);
            double R = 6371;
            return 2 * R * Math.Atan2(Math.Sqrt(alpha), Math.Sqrt(1 - alpha));
        }

        static int GetLineCount(string fileName)
        {
            StreamReader s = new StreamReader(fileName);
            int lineCount = 0;
            while (!s.EndOfStream)
            {
                s.ReadLine();
                lineCount++;
            }

            return lineCount;
        }

        static List<TaxiCourse> LoadDataSet(string filePath, ref double[] minBound, ref double[] maxBound, ref double averageSequenceLength, int lineCount = -1)
        {
            List<TaxiCourse> dataset = new List<TaxiCourse>();
            Console.WriteLine("Loading dataset from " + filePath);
            TextFieldParser csvReader = new TextFieldParser(filePath);

            //Count lines
            if (lineCount == -1)
            {
                Console.WriteLine("Counting lines...");
                lineCount = GetLineCount(filePath);
                Console.WriteLine("Lines=" + lineCount);
            }

            csvReader.SetDelimiters(new string[] { "," });
            csvReader.HasFieldsEnclosedInQuotes = true;
            string[] colFields = csvReader.ReadFields();//Discard headers

            int currentLine = 0;
            int LINES_TO_READ = lineCount;// int.MaxValue;
            double avgTrajectoryLenght = 0;
            if (minBound != null && maxBound != null)
            {
                minBound = new double[2] { double.PositiveInfinity, double.PositiveInfinity };
                maxBound = new double[2] { double.NegativeInfinity, double.NegativeInfinity };
            }
            while (!csvReader.EndOfData && currentLine < LINES_TO_READ)
            {
                string[] fieldData = csvReader.ReadFields();
                dataset.Add(new TaxiCourse(fieldData));
                avgTrajectoryLenght += dataset.Last().polyline.Count;

                if (minBound!=null &&maxBound!=null)
                {
                    foreach (double[] point in dataset.Last().polyline)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (minBound[i] > point[i])
                                minBound[i] = point[i];
                            if (maxBound[i] < point[i])
                                maxBound[i] = point[i];
                        }
                    }
                }
                currentLine++;
                if (currentLine % 100 == 0)
                {
                    double progress = 100 * currentLine / (double)lineCount;
                    Console.WriteLine(currentLine + " (" + progress + "%)");
                }
            }
            avgTrajectoryLenght /= lineCount;
            csvReader.Close();

            Console.WriteLine("Loading Complete. Average sequence lenght:" + avgTrajectoryLenght);
            averageSequenceLength = avgTrajectoryLenght;
            return dataset;
        }

        static void ScaleTrajectoryData(List<TaxiCourse> data, double[] minBounds, double[] maxBounds)
        {
            double[] range = new double[] { maxBounds[0] - minBounds[0], maxBounds[1] - minBounds[1] };

            foreach (TaxiCourse course in data)
            {
                for (int i = 0; i < course.polyline.Count; i++)
                {
                    course.polyline[i][0] = (course.polyline[i][0] - minBounds[0]) / range[0];
                    course.polyline[i][1] = (course.polyline[i][1] - minBounds[1]) / range[1];
                }
            }
        }
        static void AddTerminalSymbol(List<TaxiCourse> data, double[] symbols)
        {
            foreach (TaxiCourse course in data)
            {
                course.polyline.Add(symbols);
            }
        }
    }
}
