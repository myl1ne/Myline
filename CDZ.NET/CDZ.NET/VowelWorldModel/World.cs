using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CDZNET;
using System.Drawing;

namespace VowelWorldModel
{
    class World
    {
        Random rnd = new Random();
        public int Width { get { return cells.GetLength(0); } }
        public int Height { get { return cells.GetLength(1); } }

        int shapesCount;
        double orientationVariability;

        public  Cell[,] cells;

        readonly double[] CODE_RED = { 1.0, 0.0, 0.0, 0.0 };
        readonly double[] CODE_BLUE = { 0.0, 1.0, 0.0, 0.0 };
        readonly double[] CODE_YELLOW = { 0.0, 0.0, 1.0, 0.0 };
        readonly double[] CODE_GREEN = { 0.0, 0.0, 0.0, 1.0 };

        public World(int w, int h, int shapesCount = 4, double orientationVariability = 10.0f)
        {
            cells = new Cell[w, h];
            this.orientationVariability = orientationVariability;
            this.shapesCount = shapesCount;
        }
        public double[] getColorCodeFromValue(double value)
        {
            double[] colorCode;
            if (value < 1.0 / 4.0)          //BLUE
                colorCode = CODE_BLUE;
            else if (value < 1.0 / 2.0)     //GREEN
                colorCode = CODE_GREEN;
            else if (value < 3.0 / 4.0)     //RED
                colorCode = CODE_RED;
            else                                    //YELLOW
                colorCode = CODE_YELLOW; ;

            return colorCode;
        }
        public void Randomize(int seedsCount)
        {
            Console.WriteLine("Randomizing world with " + seedsCount);
            cells = new Cell[Width, Height];

            //Plant seeds
            for (int i = 0; i < seedsCount; i++)
                computeCell(rnd.Next(0, Width), rnd.Next(0, Height));

            //Fill the matrix
            bool isMatrixFilled = false;
            while (!isMatrixFilled)
            {
                List<Point2D> notInitializedNeighbors = new List<Point2D>();

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        //This cell is initialized
                        if (cells[i, j] != null)
                        {
                            for (int i2 = -1; i2 <= 1; i2++)
                            {
                                for (int j2 = -1; j2 <= 1; j2++)
                                {
                                    //Make it a donut shape
                                    int fI = (i + i2) % Width;
                                    int fJ = (j + j2) % Height;
                                    fI = (fI < 0) ? Width + fI : fI;
                                    fJ = (fJ < 0) ? Height + fJ : fJ;
                                    //Debug.Log("Checking " + i + "+" + i2 + " ; " + j + "+" + j2 + " = " + fI + ";" + fJ);
                                    if (cells[fI, fJ] == null)
                                    {
                                        notInitializedNeighbors.Add(new Point2D(fI, fJ));
                                        isMatrixFilled = false;
                                    }
                                }
                            }
                        }
                    }
                }

                isMatrixFilled = (notInitializedNeighbors.Count == 0);

                //Debug.Log("Matrix Completed : " + isMatrixFilled);
                if (!isMatrixFilled)
                {
                    //Generate those cells
                    notInitializedNeighbors.Shuffle();
                    foreach (Point2D coo in notInitializedNeighbors)
                    {
                        //Just double check because we can have the same neighbor added twice
                        if (cells[(int)coo.X, (int)coo.Y] == null)
                            computeCell((int)coo.X, (int)coo.Y);
                    }
                }
            }
            Console.WriteLine("Done.");
        }

        private void computeCell(int i, int j)
        {
            //Instantiate it & keep track of xy
            cells[i, j] = new Cell();
            cells[i, j].X = i;
            cells[i, j].Y = j;

            //Detect all the neighbors      
            List<Cell> neighbors = new List<Cell>();
            for (int i2 = -1; i2 <= 1; i2++)
            {
                for (int j2 = -1; j2 <= 1; j2++)
                {
                    //Make it a donut shape              
                    int fI = (i + i2) % Width;
                    int fJ = (j + j2) % Height;
                    fI = (fI < 0) ? Width + fI : fI;
                    fJ = (fJ < 0) ? Height + fJ : fJ;

                    if ( !(i2==0&&j2==0) && cells[fI, fJ] != null)
                    {
                        neighbors.Add(cells[fI, fJ]);
                    }
                }
            }

            //----------------------------------------------------------GIST - COLOR----------------------------------------------//
            //Compute the color based on theirs
            cells[i, j].colorValue = 0.0f;
            foreach (Cell n in neighbors)
            {
                cells[i, j].colorValue += n.colorValue;
            }
            if (neighbors.Count == 0)
            {
                cells[i, j].colorValue = rnd.NextDouble();
            }
            else
            {
                cells[i, j].colorValue /= (neighbors.Count);
                //Add some variability
                cells[i, j].colorValue +=  ( rnd.NextDouble() * 0.2 - 0.1 );
            }
            MathHelpers.Clamp(ref cells[i, j].colorValue, 0.0, 1.0);
            cells[i, j].colorCode = getColorCodeFromValue(cells[i, j].colorValue);
            //Debug.Log("Frequency= " + cells[i, j].frequency);

            //----------------------------------------------------------SEMANTIC - ORIENTATION----------------------------------------------//
            //Orientation is given by color/gist
            if (cells[i, j].colorValue < 1 / 4.0)
            {
                cells[i, j].orientation = 0.0f;
            }
            else if (cells[i, j].colorValue < 1 / 2.0)
            {
                cells[i, j].orientation = 45.0f;
            }
            else if (cells[i, j].colorValue < 3 / 4.0)
            {
                cells[i, j].orientation = 90.0f;
            }
            else
            {
                cells[i, j].orientation = 135.0f;
            }
            cells[i, j].orientation += (rnd.NextDouble() * orientationVariability - orientationVariability/2.0);

            //----------------------------------------------------------SHAPE----------------------------------------------//
            //Shape is defined by syntactic rule
            //rnd for now
            cells[i, j].shape = rnd.Next(0, shapesCount);
        }

        public Dictionary<string, Bitmap> toImages()
        {
            Dictionary<string, Bitmap> bmps = new Dictionary<string, Bitmap>();
            bmps["color"] = new Bitmap(Width, Height);
            bmps["orientation"] = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    bmps["color"].SetPixel(i, j, cells[i, j].colorFromCode);
                    int orientationColor = (int)(255 * Math.Abs(cells[i, j].orientation%180.0)/180.0);
                    if (double.IsNaN(cells[i, j].orientation))
                        bmps["orientation"].SetPixel(i, j, Color.Red);
                    else
                        bmps["orientation"].SetPixel(i, j, Color.FromArgb(orientationColor,orientationColor,orientationColor));
                }               
            }
            return bmps;
        }

        public Dictionary<string, double[,]> sampleSaccade(int retinaSize, int saccadeLength)
        {
            Dictionary<string, double[,]> record = new Dictionary<string, double[,]>();

            //-----------------
            //Fixate somewhere
            int startX = rnd.Next(0, Width - retinaSize);
            int startY = rnd.Next(0, Height - retinaSize);

            //-----------------
            //Saccade
            int endX = -1; int dX = 0;
            int endY = -1; int dY = 0;
            while (endX < 0 || endY < 0 || endX >= Width - retinaSize || endY >= Height - retinaSize)
            {
                dX = rnd.Next(-saccadeLength, saccadeLength + 1);
                dY = rnd.Next(-saccadeLength, saccadeLength + 1);
                endX = startX + dX;
                endY = startY + dY;
            }

            //-----------------
            //Get the perceptive information
            //1-Proprioception
            //dX dY 
            //Right = 0 1 0 0
            //Left  = 1 0 0 0
            //Up    = 0 0 0 1
            //Down  = 0 0 1 0
            record["Saccade"] = new double[4, 1];
            if (dX == -saccadeLength)
            {
                record["Saccade"][0, 0] = 1;
                record["Saccade"][1, 0] = 0;
            }
            else if (dX == saccadeLength)
            {
                record["Saccade"][0, 0] = 0;
                record["Saccade"][1, 0] = 1;
            }
            else
            {
                record["Saccade"][0, 0] = 0;
                record["Saccade"][1, 0] = 0;
            }


            if (dY == -saccadeLength)
            {
                record["Saccade"][2, 0] = 1;
                record["Saccade"][3, 0] = 0;
            }
            else if (dY == saccadeLength)
            {
                record["Saccade"][2, 0] = 0;
                record["Saccade"][3, 0] = 1;
            }
            else
            {
                record["Saccade"][2, 0] = 0;
                record["Saccade"][3, 0] = 0;
            }

            //2-Vision
            record["XY-t0"] = new double[2, 1] { { startX }, { startY } };
            record["Vision-t0-Color"] = new double[retinaSize * 4, retinaSize];
            record["Vision-t0-Orientation"] = new double[retinaSize * 2, retinaSize];
            record["Vision-t0-Shape"] = new double[retinaSize * 4, retinaSize];
            record["XY-t1"] = new double[2, 1] { { endX }, { endY } };
            record["Vision-t1-Color"] = new double[retinaSize * 4, retinaSize];
            record["Vision-t1-Orientation"] = new double[retinaSize * 2, retinaSize];
            record["Vision-t1-Shape"] = new double[retinaSize * 4, retinaSize];
            for (int i = 0; i < retinaSize; i++)
            {
                for (int j = 0; j < retinaSize; j++)
                {
                    Cell startPx = cells[startX + i, startY + j];
                    Cell endPx = cells[endX + i, endY + j];

                    //Color (4 compo)
                    for (int compo = 0; compo < 4; compo++)
                    {
                        record["Vision-t0-Color"][i * 4 + compo, j] = startPx.colorCode[compo];
                        record["Vision-t1-Color"][i * 4 + compo, j] = endPx.colorCode[compo];
                    }

                    //Orientation (2 compo)
                    for (int compo = 0; compo < 2; compo++)
                    {
                        record["Vision-t0-Orientation"][i * 2 + compo, j] = startPx.orientationCode[compo];
                        record["Vision-t1-Orientation"][i * 2 + compo, j] = endPx.orientationCode[compo];
                    }

                    //Shape (4 compo)
                    for (int compo = 0; compo < 4; compo++)
                    {
                        record["Vision-t0-Shape"][i * 4 + compo, j] = (compo == startPx.shape) ? 1 : 0;
                        record["Vision-t1-Shape"][i * 4 + compo, j] = (compo == endPx.shape) ? 1 : 0;
                    }
                }
            }
            return record;
        }

        public Dictionary<string, List<Dictionary<string, double[,]>>> generateSaccadeDatasets(int dataSetSize, int retinaSize, int saccadeLength)
        {
            Dictionary<string, List<Dictionary<string, double[,]>>> datasets = new Dictionary<string, List<Dictionary<string, double[,]>>>();
            datasets["train"] = new List<Dictionary<string, double[,]>>();
            datasets["test"] = new List<Dictionary<string, double[,]>>();
            for (int step = 0; step < dataSetSize; step++)
            {
                datasets["train"].Add(sampleSaccade(retinaSize, saccadeLength));
                datasets["test"].Add(sampleSaccade(retinaSize, saccadeLength));
            }
            return datasets;
        }
    }
}
