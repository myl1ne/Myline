using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using CDZNET;
using CDZNET.Core;

namespace VowelWorldModel
{
    public partial class MainWindow : Form
    {
        //Parameters
        int retinaSize = 3;
        int shapeCount = 4;
        int worldWidth = 100;
        int worldHeight = 100;
        int seedsNumber = 5;
        double orientationVariability = 0.0; //degrees
        World world;
        Bitmap worldVisu;

        //Color codes
        readonly double[] CODE_RED = { 0.0, 0.0, 0.0, 1.0 };
        readonly double[] CODE_BLUE = { 0.0, 0.0, 1.0, 0.0 };
        readonly double[] CODE_GREEN = { 0.0, 1.0, 0.0, 0.0 };
        readonly double[] CODE_YELLOW = { 1.0, 0.0, 0.0, 0.0 };

        //Network structures
        //1-Inputs
        CDZNET.Core.Signal LEC_Color; //Visual matrix
        CDZNET.Core.Signal LEC_Orientation; //Visual matrix
        CDZNET.Core.Signal LEC_Shape; //Visual matrix
        CDZNET.Core.Signal MEC;//Proprioception/Grid Cells

        //2-Areas
        CDZNET.Core.MMNode CA3;

        //Thread
        public Thread mainLoopThread; 

        public MainWindow()
        {
            InitializeComponent();

            //Generate the world
            world = new World(worldWidth, worldHeight, shapeCount, orientationVariability);
            world.Randomize(seedsNumber);
            getWorldVisualization();

            //Generate the network
            //1-Inputs
            LEC_Color = new CDZNET.Core.Signal(retinaSize*4, retinaSize); //Visual matrix (RED=0001 / BLUE=0010 / GREEN=0100 / YELLOW=1000)
            LEC_Orientation = new CDZNET.Core.Signal(retinaSize, retinaSize); //Visual matrix
            LEC_Shape = new CDZNET.Core.Signal(retinaSize, retinaSize); //Visual matrix
            MEC = new CDZNET.Core.Signal(2, 1); //Proprioception/Grid Cells

            //2-Areas          
            CA3 = new CDZNET.Core.MMNodeLookupTable(new Point2D(1, 1)); //Here you specify which algo to be used
            (CA3 as MMNodeLookupTable).TRESHOLD_SIMILARITY = 0.1;
            (CA3 as MMNodeLookupTable).learningRate = 0.1;

            //CA3 = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(50, 50), false); //Here you specify which algo to be used
            //(CA3 as MMNodeSOM).learningRate = 0.3;
            //(CA3 as MMNodeSOM).elasticity = 10.0;
            //(CA3 as MMNodeSOM).activityRatioToConsider = 1.0;

            //Define which signal will enter CA3
            CA3.addModality(LEC_Color, "Color");
            CA3.addModality(LEC_Orientation, "Orientation");
            //CA3.addModality(LEC_Shape, "Shape");
            //CA3.addModality(MEC, "XY");

            //Gui purpose
            ctrlCA3.attach(CA3);

            mainLoopThread = new Thread(mainLoop);
            mainLoopThread.Start();
        }

        public void mainLoop()
        {
            //Explore the world
            Random rnd = new Random();
            StreamWriter logFile = new StreamWriter("errorLog.csv");
            //logFile.WriteLine("t,realColor0,realColor1,realColor2,realColor3,realOrientation, predColor0,predColor1,predColor2,predColor3, predOrientation,orientation->color0,orientation->color1,orientation->color2,orientation->color3, color->orientation, Ecolor,Eorientation");
            logHeadings(logFile);
            //int explorationSteps = 1000;
            //for (int step = 0; step < explorationSteps; step++)
            int steps = 0;
            while (steps<1000 && mainLoopThread.IsAlive)
            {
                //-----------------
                //Saccade somewhere
                int nextX = rnd.Next(retinaSize / 2, world.Width - retinaSize / 2);
                int nextY = rnd.Next(retinaSize / 2, world.Height - retinaSize / 2);
                drawFixationPoint(nextX, nextY);

                //-----------------
                //Get the perceptive information
                //1-Proprioception
                MEC.reality[0, 0] = nextX / (double)world.Width; //We want in range [0,1]
                MEC.reality[1, 0] = nextY / (double)world.Height; //We want in range [0,1]
                //2-Vision
                for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
                {
                    for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                    {
                        Cell px = world.cells[nextX + i, nextY+ j];

                        double[] colorCode;
                        if (px.colorValue < 1.0 / 4.0)          //BLUE
                            colorCode = CODE_BLUE;
                        else if (px.colorValue < 1.0 / 2.0)     //GREEN
                            colorCode = CODE_GREEN;
                        else if (px.colorValue < 3.0 / 4.0)     //RED
                            colorCode = CODE_RED;
                        else                                    //YELLOW
                            colorCode = CODE_YELLOW; ;

                        for (int compo = 0; compo < 4; compo++)
                        {
                            LEC_Color.reality[retinaSize / 2 + i + compo, retinaSize / 2 + j] = colorCode[compo];
                        }

                        //Other features
                        LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = Math.Abs(px.orientation % 180.0) / 180.0;
                        LEC_Shape.reality[retinaSize / 2 + i, retinaSize / 2 + j] = px.shape / (double)shapeCount; //  !!! This encoding is bad because it defines intrinsic shape distance
                    }
                }

                //-----------------
                //Run a cycle on CA3
                CA3.Converge();
                CA3.Diverge();

                log(logFile, steps);

                steps++;
            }


            //-------------------------------------------------------------------------------------------------------------------
            //Test Phase
            //1--Make sure we do not modify the network
            if (CA3 is MMNodeLookupTable)
            {
                (CA3 as MMNodeLookupTable).allowTemplateCreation = false;
                (CA3 as MMNodeLookupTable).learningRate = 0.0;
            }
            if (CA3 is MMNodeSOM)
            {
                (CA3 as MMNodeSOM).learningRate = 0.0;
            }

            //2-- Use a never encountered equidistant color (i.e (0 0 0 0) )
            for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
                for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                    for (int compo = 0; compo < 4; compo++)
                        LEC_Color.reality[retinaSize / 2 + i + compo, retinaSize / 2 + j] = 0.0;

            //Test with different orientations
            for (double orientation = 0; orientation <= 1.0; orientation+=0.1)
            {
                for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
                    for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                        LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = orientation;

                CA3.Converge();
                CA3.Diverge();
                log(logFile, -1);
            }

            MessageBox.Show("Testing done.");
            //-------------------------------------------------------------------------------------------------------------------

            logFile.Close();
        }

        string GetString(double[,] a, string colDelim = ",", string rowDelim = ",")
        {
            string s = "";
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    s += a[i, j].ToString();
                    if (j != a.GetLength(1) - 1)
                        s += rowDelim;   
                } 

                if (i != a.GetLength(0) - 1)
                    s += colDelim;    
            }
            return s;
        }

        void logHeadings(StreamWriter logFile)
        {
            logFile.Write("t,");
            logFile.Write(getMatrixHeadingS("realColor", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("realOrientation", retinaSize, retinaSize, 1));
            logFile.Write(getMatrixHeadingS("fullPColor", retinaSize, retinaSize,4));
            logFile.Write(getMatrixHeadingS("fullPOrientation", retinaSize, retinaSize, 1));
            logFile.Write(getMatrixHeadingS("partialPColor", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("partialPOrientation", retinaSize, retinaSize, 1));
            logFile.WriteLine();
            //logFile.WriteLine("t,realColor0,realColor1,realColor2,realColor3,realOrientation, predColor0,predColor1,predColor2,predColor3, predOrientation,orientation->color0,orientation->color1,orientation->color2,orientation->color3, color->orientation");
        }

        string getMatrixHeadingS(string root, int w, int h, int channels = 4)
        {
            string str = "";
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        str += root + "_" + i + "_" + j + "_" + c + ",";
                    }
                }
            }
            return str;
        }

        void log(StreamWriter logFile, int recordingCount)
        {
            Dictionary<Signal, double[,]> fullPredictionError = CA3.Evaluate( CA3.modalities ); //Evaluate with all modalities
            double[,] orientationFromAll = LEC_Orientation.prediction.Clone() as double[,];
            double[,] colorFromAll = LEC_Color.prediction.Clone() as double[,];

            //Only from orientation
            Dictionary<Signal, double[,]> colorFromOrientationError = CA3.Evaluate(new List<CDZNET.Core.Signal>() { LEC_Orientation });
            double[,] colorFromOrientation = LEC_Color.prediction.Clone() as double[,];

            //Only from color
            Dictionary<Signal, double[,]> orientationFromColorError = CA3.Evaluate(new List<CDZNET.Core.Signal>() { LEC_Color });
            double[,] orientationFromColor = LEC_Orientation.prediction.Clone() as double[,];


            logFile.WriteLine(
                recordingCount + "," +
                GetString(LEC_Color.reality) + "," +
                GetString(LEC_Orientation.reality) + "," +
                GetString(colorFromAll) + "," +
                GetString(orientationFromAll) + "," +
                GetString(colorFromOrientation) + "," +
                GetString(orientationFromColor)// + "," +
                //GetSumSquared(colorFromOrientationError[LEC_Color]) + "," +
                //GetSumSquared(orientationFromColorError[LEC_Orientation]) 
                );

            logFile.Flush();
        }

        double GetSumSquared(double[,] a)
        {
            double error = 0.0;
            CDZNET.Helpers.ArrayHelper.ForEach(a, false, (x, y) => { error += Math.Pow(a[x, y], 2.0); });
            return error;
        }

        //---------------------VISUALIZATION------------------
        #region Visualuzation
        void getWorldVisualization()
        {
            worldVisu = new Bitmap(world.Width, world.Height);
            for (int i = 0; i < world.Width; i++)
            {
                for (int j = 0; j < world.Height; j++)
                {
                    Color c;
                    if (world.cells[i, j].colorValue < 1.0 / 4.0)
                        c = Color.Blue;
                    else if (world.cells[i, j].colorValue < 1.0 / 2.0)
                        c = Color.Green;
                    else if (world.cells[i, j].colorValue < 3.0 / 4.0)
                        c = Color.Red;
                    else
                        c = Color.Yellow;
                    worldVisu.SetPixel(i, j, c);
                }             
            }
            pictureBoxWorld.Image = worldVisu.Clone() as Bitmap;
            pictureBoxWorld.Refresh();
        }

        void drawFixationPoint(int x, int y)
        {
            if (pictureBoxWorld.InvokeRequired)
                this.Invoke(new Action<int, int>(drawFixationPoint), x, y);
            else
            {
                pictureBoxWorld.Image = worldVisu.Clone() as Bitmap;
                using (Graphics g = Graphics.FromImage(pictureBoxWorld.Image))
                    g.DrawRectangle(new Pen(Color.Black, 2), x - retinaSize / 2, y - retinaSize / 2, retinaSize, retinaSize);
                pictureBoxWorld.Refresh();
            }
        }
        #endregion
    }
}
