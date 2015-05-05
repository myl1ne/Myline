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
            //Load the training set
            Console.WriteLine("Loading CSV file.");
            List<TaxiCourse> trainSet = new List<TaxiCourse>();
            string filePath = "C:\\Users\\Stephane\\Documents\\GitHub\\Myline\\Datasets\\Kaggle Taxis\\train.csv";

            TextFieldParser csvReader = new TextFieldParser(filePath);

            //Count lines
            Console.WriteLine("Counting lines...");
            int lineCount = 10000;// GetLineCount(filePath);
            Console.WriteLine("Lines=" + lineCount);

            csvReader.SetDelimiters(new string[] { "," });
            csvReader.HasFieldsEnclosedInQuotes = true;
            string[] colFields = csvReader.ReadFields();//Discard headers

            int currentLine = 0;
            int LINES_TO_READ = lineCount;// int.MaxValue;
            double avgTrajectoryLenght = 0;
            while(!csvReader.EndOfData && currentLine<LINES_TO_READ)
            {
                string[] fieldData = csvReader.ReadFields();
                trainSet.Add(new TaxiCourse(fieldData));
                avgTrajectoryLenght += trainSet.Last().polyline.Count;

                currentLine++;
                if (currentLine % 100 == 0)
                {
                    double progress = 100 * currentLine / (double)lineCount;
                    Console.WriteLine(currentLine + " ("+progress + "%)");
                }
            }
            avgTrajectoryLenght /= lineCount;
            Console.WriteLine("Loading Complete. Average sequence lenght:"+avgTrajectoryLenght);

            //----------------------------------------------------------------------------------------------
            //Create the network
            TimeCells.TimeCanvas canvas = new TimeCanvas(2, 50);

            Stopwatch watch1 = new Stopwatch();
            watch1.Start();
            int EPOCH_TO_RUN = 1000;
            for (int iteration = 0; iteration < 1000; iteration++)
            {
                Stopwatch watch2 = new Stopwatch();
                watch2.Start();
                foreach(TaxiCourse course in trainSet)
                {
                    canvas.Reset();
                    canvas.Train(course.polyline);
                }
                watch2.Stop();
                Console.WriteLine("Training set (" + lineCount + " elements) processed in " + watch2.Elapsed);
            }
            watch1.Stop();
            Console.WriteLine("Training set (" + lineCount + " elements) x " + EPOCH_TO_RUN + " epochs processed in " + watch1.Elapsed);
        }

        private static int GetLineCount(string fileName)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(fileName));
            int lineCount = 0;

            char lastChar = reader.ReadChar();
            char newChar = new char();

            do
            {
                newChar = reader.ReadChar();
                if (lastChar == '\r' && newChar == '\n')
                {
                    lineCount++;
                }
                lastChar = newChar;
            } while (reader.PeekChar() != -1);

            return lineCount;
        }
    }
}
