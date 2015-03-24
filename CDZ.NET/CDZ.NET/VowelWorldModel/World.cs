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
    }
}
