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
using CDZNET.GUI;

namespace VowelWorldModel
{
    public partial class Saccades : Form
    {
        Random rnd = new Random();
        //Parameters
        int retinaSize = 9;
        int shapeCount = 4;
        int worldWidth = 100;
        int worldHeight = 100;
        int seedsNumber = 3;
        int trainSteps = 1000;
        int saccadeMaximumRange = 3;
        double orientationVariability = 0.0; //degrees
        World world;
        Dictionary<string, Bitmap> worldVisu;

        //Network structures
        //1-Inputs
        CDZNET.Core.Signal LEC_ColorT0; //Visual matrix
        CDZNET.Core.Signal LEC_ColorT1; //Visual matrix
        CDZNET.Core.Signal MEC;//Proprioception/Grid Cells

        //2-Areas
        CDZNET.Core.MMNode CA3;

        //Thread
        //public Thread mainLoopThread; 

        public Saccades()
        {
            InitializeComponent();

            //Generate the world
            world = new World(worldWidth, worldHeight, shapeCount, orientationVariability);
            world.Randomize(seedsNumber);
            getWorldVisualization();

            //Generate the network
            //1-Inputs
            LEC_ColorT0 = new CDZNET.Core.Signal(retinaSize*4, retinaSize); //Visual matrix (RED=0001 / BLUE=0010 / GREEN=0100 / YELLOW=1000)          
            LEC_ColorT1 = new CDZNET.Core.Signal(retinaSize*4, retinaSize); //Visual matrix (RED=0001 / BLUE=0010 / GREEN=0100 / YELLOW=1000)

            MEC = new CDZNET.Core.Signal(2, 1); //dX dY

            //2-Areas          
            CA3 = new CDZNET.Core.MMNodeLookupTable(new Point2D(1, 1)); //Here you specify which algo to be used
            (CA3 as MMNodeLookupTable).TRESHOLD_SIMILARITY = 0.1;
            (CA3 as MMNodeLookupTable).learningRate = 0.1;

            //CA3 = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(20, 20), false); //Here you specify which algo to be used
            //(CA3 as MMNodeSOM).learningRate = 0.01;
            //(CA3 as MMNodeSOM).elasticity = 5.0;
            //(CA3 as MMNodeSOM).activityRatioToConsider = 1.0;

            //Define which signal will enter CA3
            CA3.addModality(LEC_ColorT0, "ColorT0");
            CA3.addModality(LEC_ColorT1, "ColorT1");
            //CA3.addModality(LEC_Shape, "Shape");
            CA3.addModality(MEC, "dXdY");

            //Gui purpose
            ctrlCA3.attach(CA3);

            //mainLoopThread = new Thread(mainLoop);
            //mainLoopThread.Start();
        }

        public void learnAndLog()
        {
            //Explore the world
            StreamWriter logFile = new StreamWriter("errorLogSaccades.csv");
            logHeadings(logFile);

            int steps = 0;
            int worldCnt = 0;
            progressBarCurrentOp.Minimum = 0;
            progressBarCurrentOp.Maximum = 3 * trainSteps;
            progressBarCurrentOp.Step = 1;
            while (steps<2*trainSteps)
            {
                progressBarCurrentOp.PerformStep();
                if (steps > trainSteps)
                {
                    CA3.learningLocked = true;
                    //if (steps==2*trainSteps)
                    //{
                    //    world.Randomize(seedsNumber);
                    //    getWorldVisualization();
                    //    worldCnt++;
                    //}
                }
                step(logFile, steps, worldCnt);
                steps++;
            }
            logFile.Close();
        }

        void step(StreamWriter logFile, int steps, int worldCnt)
        {
            //-----------------
            //Fixate somewhere
            int startX = rnd.Next(retinaSize / 2, world.Width - retinaSize / 2);
            int startY = rnd.Next(retinaSize / 2, world.Height - retinaSize / 2);

            //-----------------
            //Saccade
            int endX = -1; int dX = 0;
            int endY = -1; int dY = 0;
            while (endX < retinaSize / 2 || endY < retinaSize / 2 || endX >= world.Width - retinaSize / 2 || endY >= world.Width - retinaSize / 2)
            {
                dX = rnd.Next(-saccadeMaximumRange, saccadeMaximumRange);
                dY = rnd.Next(-saccadeMaximumRange, saccadeMaximumRange);
                endX = startX + dX;
                endY = startY + dY;
            }

            //-----------------
            //Get the perceptive information
            //1-Proprioception
            MEC.reality[0, 0] = (dX + saccadeMaximumRange) / (2.0 * saccadeMaximumRange); //We want in range [0,1]
            MEC.reality[1, 0] = (dY + saccadeMaximumRange) / (2.0 * saccadeMaximumRange); //We want in range [0,1]

            //2-Vision
            for (int i = -retinaSize / 2; i <= retinaSize / 2; i++)
            {
                for (int j = -retinaSize / 2; j <= retinaSize / 2; j++)
                {
                    Cell startPx = world.cells[startX + i, startY + j];
                    Cell endPx = world.cells[endX + i, endY + j];

                    for (int compo = 0; compo < 4; compo++)
                    {
                        LEC_ColorT0.reality[(retinaSize / 2 + i) * 4 + compo, retinaSize / 2 + j] = startPx.colorCode[compo];
                        LEC_ColorT1.reality[(retinaSize / 2 + i) * 4 + compo, retinaSize / 2 + j] = endPx.colorCode[compo];
                    }
                }
            }

            //-----------------
            //Run a cycle on CA3
            CA3.Converge();
            CA3.Diverge();
            log(logFile, steps, !CA3.learningLocked, worldCnt);
        }

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
            logFile.Write("learning,");
            logFile.Write("world,");
            logFile.Write(getMatrixHeadingS("realColorT0", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("realColorT1", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("realSaccade", 1, 1, 2));
            logFile.Write(getMatrixHeadingS("c0sacc->c1", retinaSize, retinaSize, 4));
            logFile.Write("ERRORc0sacc->c1,");
            logFile.Write(getMatrixHeadingS("c1sacc->c0", retinaSize, retinaSize, 4));
            logFile.Write("ERRORc1sacc->c0,");
            logFile.Write(getMatrixHeadingS("c0c1->sacc", 1, 1, 2));
            logFile.Write("ERRORc0c1->sacc,");
            logFile.WriteLine();
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

        void log(StreamWriter logFile, int recordingCount, bool learning, int world)
        {
            //Dictionary<Signal, double[,]> fullPrediction = CA3.Predict( CA3.modalities ); //Evaluate with all modalities
            //c0sacc->c1
            Dictionary<Signal, double[,]> c1Pred = CA3.Predict(new List<CDZNET.Core.Signal>() { LEC_ColorT0, MEC });
            //c1sacc->c0
            Dictionary<Signal, double[,]> c0Pred = CA3.Predict(new List<CDZNET.Core.Signal>() { LEC_ColorT1, MEC });
            //c0c1->sacc
            Dictionary<Signal, double[,]> saccPred = CA3.Predict(new List<CDZNET.Core.Signal>() { LEC_ColorT0, LEC_ColorT1 });

            pictureBoxEndPoint.Image = getBitmapFromColorCode(LEC_ColorT1.reality);
            pictureBoxPredictedEndpoint.Image = getBitmapFromColorCode(c1Pred[LEC_ColorT1]);
            pictureBoxEndPoint.Refresh();
            pictureBoxPredictedEndpoint.Refresh();
            if (logFile != null)
            {
                logFile.WriteLine(
                    recordingCount + "," +
                    learning + "," +
                    world + "," +
                    GetString(LEC_ColorT0.reality) + "," +
                    GetString(LEC_ColorT1.reality) + "," +
                    GetString(MEC.reality) + "," +
                    GetString(c1Pred[LEC_ColorT1]) + "," +
                    GetSumSquared(c1Pred[LEC_ColorT1], LEC_ColorT1.reality) + "," +
                    GetString(c0Pred[LEC_ColorT0]) + "," +
                    GetSumSquared(c0Pred[LEC_ColorT0], LEC_ColorT0.reality) + "," +
                    GetString(saccPred[MEC]) + "," +
                    GetSumSquared(saccPred[MEC], MEC.reality)
                    );

                logFile.Flush();
            }
        }

        double GetSumSquared(double[,] truth, double[,] estimate)
        {
            double error = 0.0;
            CDZNET.Helpers.ArrayHelper.ForEach(truth, false, (x, y) => { error += Math.Pow(truth[x, y] - estimate[x,y], 2.0); });
            return error;
        }

        //---------------------VISUALIZATION------------------
        #region Visualization

        Bitmap getBitmapFromColorCode(double[,] a)
        {
            Bitmap bmp = new Bitmap(a.GetLength(0) / 4, a.GetLength(1));
            for (int i = 0; i < a.GetLength(0); i+=4)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    bmp.SetPixel(i/4, j, Cell.getColorFromCode(new double[] { a[i, j], a[i + 1, j], a[i + 2, j], a[i + 3, j] }));
                }
            }
            return bmp;
        }

        void getWorldVisualization()
        {
            worldVisu = world.toImages();
            pictureBoxWorldColor.Image = worldVisu["color"].Clone() as Bitmap;
            pictureBoxWorldColor.Refresh();;
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

        private void button1_Click(object sender, EventArgs e)
        {
            step(null, -1, -1);
        }

        private void buttonRdmz_Click(object sender, EventArgs e)
        {
            world.Randomize(seedsNumber);
            getWorldVisualization();
        }
    }
}
