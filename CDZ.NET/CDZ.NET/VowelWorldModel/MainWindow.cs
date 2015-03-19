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
        enum Model { SOM, LUT, Matlab };
        Model modelUsed = Model.Matlab;
        int retinaSize = 1;
        int shapeCount = 4;
        int worldWidth = 100;
        int worldHeight = 100;
        int seedsNumber = 3;
        int trainSteps = 5000;
        double orientationVariability = 0.0; //degrees
        World world;
        Dictionary<string, Bitmap> worldVisu;

        //Network structures
        //1-Inputs
        CDZNET.Core.Signal LEC_Color; //Visual matrix
        CDZNET.Core.Signal LEC_Orientation; //Visual matrix
        CDZNET.Core.Signal LEC_Shape; //Visual matrix
        CDZNET.Core.Signal MEC;//Proprioception/Grid Cells

        //2-Areas
        CDZNET.Core.MMNode CA3;

        //Thread
        //public Thread mainLoopThread; 

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
            switch (modelUsed)
            {
                case Model.SOM:
                    CA3 = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(20, 20), false); //Here you specify which algo to be used
                    (CA3 as MMNodeSOM).learningRate = 0.03;
                    (CA3 as MMNodeSOM).elasticity = 2.0;
                    (CA3 as MMNodeSOM).activityRatioToConsider = 1.0;
                    break;

                case Model.LUT:
                    CA3 = new CDZNET.Core.MMNodeLookupTable(new Point2D(1, 1)); //Here you specify which algo to be used
                    (CA3 as MMNodeLookupTable).TRESHOLD_SIMILARITY = 0.1;
                    (CA3 as MMNodeLookupTable).learningRate = 0.1;
                    break;

                case Model.Matlab:
                    CA3 = new CDZNET.Core.MMNodeMatLab(new CDZNET.Point2D(1, 1),            //This is the size of the output (so far not set in matlab case)
                        "CA3",                                                              //This is the name of the variable corresponding to this node in Matlab
                        "D:/robotology/src/Myline/CDZ.NET/CDZ.NET/CDZ.NET/Core/MM/Matlab",  //Path where the script is located
                        "dummyConvergenceDivergence"                                        //name of the function/script
                        );
                    break;

                default:
                    throw new Exception("Unknown model type.");
            }

            //Define which signal will enter CA3
            CA3.addModality(LEC_Color, "Color");
            CA3.addModality(LEC_Orientation, "Orientation");
            //CA3.addModality(LEC_Shape, "Shape");
            CA3.addModality(MEC, "XY");

            //Gui purpose
            ctrlCA3.attach(CA3);

            //mainLoopThread = new Thread(mainLoop);
            //mainLoopThread.Start();
        }

        public void learnAndLog()
        {
            Random rnd = new Random();

            if (checkBoxPretrainMotor.Checked)
            {
                //We train only the motor map
                foreach(Signal s in CA3.modalities)
                {
                    if (s != MEC)
                    {
                        CA3.modalitiesInfluence[s] = 0.0;
                    }
                }

                //Pretrain the motor modality
                int pretrainSteps = 1000;
                progressBarCurrentOp.Value = 0;
                progressBarCurrentOp.Minimum = 0;
                progressBarCurrentOp.Maximum = pretrainSteps;
                progressBarCurrentOp.Step = 1;

                for (int i = 0; i < pretrainSteps; i++)
                {
                    MEC.reality[0, 0] = rnd.NextDouble();
                    MEC.reality[1, 0] = rnd.NextDouble();
                    CA3.Converge();
                    CA3.Diverge();
                    progressBarCurrentOp.PerformStep();
                }

                //Put all influences back to 1
                foreach (Signal s in CA3.modalities)
                {
                    CA3.modalitiesInfluence[s] = 1.0;
                }
            }

            //Explore the world
            StreamWriter logFile = new StreamWriter("errorLog.csv");
            //logFile.WriteLine("t,realColor0,realColor1,realColor2,realColor3,realOrientation, predColor0,predColor1,predColor2,predColor3, predOrientation,orientation->color0,orientation->color1,orientation->color2,orientation->color3, color->orientation, Ecolor,Eorientation");
            logHeadings(logFile);
            //int explorationSteps = 1000;
            //for (int step = 0; step < explorationSteps; step++)
            int steps = 0;
            while (steps<trainSteps /*&& mainLoopThread.IsAlive*/)
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

                        for (int compo = 0; compo < 4; compo++)
                        {
                            LEC_Color.reality[(retinaSize / 2 + i) * 4 + compo, retinaSize / 2 + j] = px.colorCode[compo];
                        }

                        //Other features
                        //LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = Math.Abs(px.orientation % 180.0) / 180.0;
                        LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = px.orientation / 180.0;
                        LEC_Shape.reality[retinaSize / 2 + i, retinaSize / 2 + j] = px.shape / (double)shapeCount; //  !!! This encoding is bad because it defines intrinsic shape distance
                    }
                }

                //-----------------
                //Run a cycle on CA3
                CA3.Converge();
                CA3.Diverge();

                log(logFile, steps);
                if (checkBoxEgosphere.Checked)
                {
                    predictEgosphere();
                    //MessageBox.Show("top");
                }
                steps++;
            }


            //-------------------------------------------------------------------------------------------------------------------
            //Test Phase
            //1--Make sure we do not modify the network
            //if (CA3 is MMNodeLookupTable)
            //{
            //    (CA3 as MMNodeLookupTable).allowTemplateCreation = false;
            //    (CA3 as MMNodeLookupTable).learningRate = 0.0;
            //}
            //if (CA3 is MMNodeSOM)
            //{
            //    (CA3 as MMNodeSOM).learningRate = 0.0;
            //}
            CA3.learningLocked = true;

            //2-- Use a never encountered equidistant color (i.e (0 0 0 0) )
            for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
                for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                    for (int compo = 0; compo < 4; compo++)
                        LEC_Color.reality[(retinaSize / 2 + i) * 4 + compo, retinaSize / 2 + j] = 0.0;

            //Test with different orientations
            for (double orientation = 0; orientation <= 1.0; orientation+=0.025)
            {
                for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
                    for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                        LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = orientation;

                CA3.Converge();
                CA3.Diverge();
                log(logFile, -1);


            }

            predictEgosphere();

            MessageBox.Show("Testing done.");
            //-------------------------------------------------------------------------------------------------------------------

            logFile.Close();
        }

        void predictEgosphere()
        {
            CA3.learningLocked = true;
            World predictedWorld = new World(world.Width, world.Height);

            progressBarCurrentOp.Value = 0;
            progressBarCurrentOp.Minimum = 0;
            progressBarCurrentOp.Maximum = world.Width * world.Height;
            progressBarCurrentOp.Step = 1;
            //We test the predictions of MEC only
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    predictedWorld.cells[x, y] = new Cell();
                    if (x >= retinaSize / 2 && x < world.Width - retinaSize / 2 && y >= retinaSize / 2 && y < world.Height - retinaSize / 2)
                    {
                        MEC.reality[0, 0] = x / (double)world.Width; //We want in range [0,1]
                        MEC.reality[1, 0] = y / (double)world.Height; //We want in range [0,1]
                        Dictionary<Signal, double[,]> prediction = CA3.Predict(new List<Signal> { MEC });

                        //We use only the center as the prediction
                        predictedWorld.cells[x, y].orientation = prediction[LEC_Orientation][retinaSize / 2, retinaSize / 2] * 180.0;
                        for (int compo = 0; compo < 4; compo++)
                            predictedWorld.cells[x, y].colorCode[compo] = prediction[LEC_Color][retinaSize / 2 + compo, retinaSize / 2];

                        //predictedWorld.cells[x, y].frequency
                        progressBarCurrentOp.PerformStep();
                    }
                }               
            }

            Dictionary<string, Bitmap> egoSphereVisu = predictedWorld.toImages();
            pictureBoxEgosphereColor.Image = egoSphereVisu["color"];
            pictureBoxEgosphereColor.Refresh();
            pictureBoxEgosphereOrientation.Image = egoSphereVisu["orientation"];
            pictureBoxEgosphereOrientation.Refresh();

            CA3.learningLocked = false;
        }

        //---------------------SACCADES------------------

        //---------------------LOG------------------
        string GetString(double[,] a, string colDelim = ",", string rowDelim = ",")
        {
            string s = ""; for
                (int j = 0; j < a.GetLength(1); j++)
            {
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    s += a[i, j].ToString();

                    if (i != a.GetLength(0) - 1)
                        s += colDelim;
                }

                if (j != a.GetLength(1) - 1)
                    s += rowDelim;
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
            worldVisu = world.toImages();
            pictureBoxWorldColor.Image = worldVisu["color"].Clone() as Bitmap;
            pictureBoxWorldColor.Refresh();
            pictureBoxWorldOrientation.Image = worldVisu["orientation"].Clone() as Bitmap;
            pictureBoxWorldOrientation.Refresh();
        }

        void drawFixationPoint(int x, int y)
        {
            if (pictureBoxWorldColor.InvokeRequired)
                this.Invoke(new Action<int, int>(drawFixationPoint), x, y);
            else
            {
                pictureBoxWorldColor.Image = worldVisu["color"].Clone() as Bitmap;
                using (Graphics g = Graphics.FromImage(pictureBoxWorldColor.Image))
                    g.DrawRectangle(new Pen(Color.Black, 2), x - retinaSize / 2, y - retinaSize / 2, retinaSize, retinaSize);
                pictureBoxWorldColor.Refresh();
            }
        }
        #endregion

        //---------------------GUI------------------
        private void buttonLearnAndLog_Click(object sender, EventArgs e)
        {
            learnAndLog();
        }
    }
}
