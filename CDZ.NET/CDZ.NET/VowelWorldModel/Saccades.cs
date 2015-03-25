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
        MNNodeFactory.Model modelUsed = MNNodeFactory.Model.DeepBelief;
        int retinaSize = 3;
        int shapeCount = 4;
        int worldWidth = 250;
        int worldHeight = 250;
        int seedsNumber = 3;
        int trainSteps = 10000;
        int saccadeSize = 1;
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

            MEC = new CDZNET.Core.Signal(4, 1); 
            //dX dY 
            //Right = 0 1 0 0
            //Left  = 1 0 0 0
            //Up    = 0 0 0 1
            //Down  = 0 0 1 0

            //2-Areas 
            CA3 = MNNodeFactory.obtain(modelUsed);

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
                    if (steps >= 2 * trainSteps && steps % trainSteps == 0)
                    {
                        world.Randomize(seedsNumber);
                        getWorldVisualization();
                        worldCnt++;
                    }
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
            int startX = rnd.Next(0, world.Width - retinaSize);
            int startY = rnd.Next(0, world.Height - retinaSize);

            //-----------------
            //Saccade
            int endX = -1; int dX = 0;
            int endY = -1; int dY = 0;
            while (endX < 0 || endY < 0 || endX >= world.Width - retinaSize || endY >= world.Width - retinaSize)
            {
                dX = rnd.Next(-saccadeSize, saccadeSize + 1);
                dY = rnd.Next(-saccadeSize, saccadeSize + 1);
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
            if (dX == -saccadeSize)
            {
                MEC.reality[0, 0] = 1;
                MEC.reality[1, 0] = 0;
            }
            else if (dX == saccadeSize)
            {
                MEC.reality[0, 0] = 0;
                MEC.reality[1, 0] = 1;
            }
            else
            {
                MEC.reality[0, 0] = 0;
                MEC.reality[1, 0] = 0;
            }


            if (dY == -saccadeSize)
            {
                MEC.reality[2, 0] = 1;
                MEC.reality[3, 0] = 0;
            }
            else if (dY == saccadeSize)
            {
                MEC.reality[2, 0] = 0;
                MEC.reality[3, 0] = 1;
            }
            else
            {
                MEC.reality[2, 0] = 0;
                MEC.reality[3, 0] = 0;
            }

            //2-Vision
            for (int i = 0; i < retinaSize; i++)
            {
                for (int j = 0; j < retinaSize; j++)
                {
                    Cell startPx = world.cells[startX + i, startY + j];
                    Cell endPx = world.cells[endX + i, endY + j];

                    for (int compo = 0; compo < 4; compo++)
                    {
                        LEC_ColorT0.reality[i* 4 + compo, j] = startPx.colorCode[compo];
                        LEC_ColorT1.reality[i* 4 + compo, j] = endPx.colorCode[compo];
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

        void logHeadings(StreamWriter logFile)
        {
            logFile.Write("t,");
            logFile.Write("learning,");
            logFile.Write("world,");
            logFile.Write(getMatrixHeadingS("realColorT0", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("realColorT1", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("realSaccade", 1, 1, 4));
            logFile.Write(getMatrixHeadingS("c0sacc->c1", retinaSize, retinaSize, 4));
            logFile.Write(getMatrixHeadingS("ERRORc0sacc->c1", retinaSize, retinaSize, 4));
            logFile.Write("ERRORleft,");
            logFile.Write("ERRORtop,");
            logFile.Write("ERRORright,");
            logFile.Write("ERRORbottom,");
            //logFile.Write(getMatrixHeadingS("c1sacc->c0", retinaSize, retinaSize, 4));
            //logFile.Write("ERRORc1sacc->c0,");
            //logFile.Write(getMatrixHeadingS("c0c1->sacc", 1, 1, 2));
            //logFile.Write("ERRORc0c1->sacc,");
            logFile.WriteLine();
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
            Dictionary<string, double> directionalErrors = getDirectionalErrors(LEC_ColorT1.reality, c1Pred[LEC_ColorT1]);
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
                    GetString(GetSquaredError(LEC_ColorT1.reality, c1Pred[LEC_ColorT1])) + "," +
                    directionalErrors["left"] + "," +
                    directionalErrors["top"] + "," +
                    directionalErrors["right"] + "," +
                    directionalErrors["bottom"]
                    //GetSumSquared(c1Pred[LEC_ColorT1], LEC_ColorT1.reality) + "," +
                    //GetString(c0Pred[LEC_ColorT0]) + "," +
                    //GetSumSquared(c0Pred[LEC_ColorT0], LEC_ColorT0.reality) + "," +
                    //GetString(saccPred[MEC]) + "," +
                    //GetSumSquared(saccPred[MEC], MEC.reality)
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
        double[,] GetSquaredError(double[,] truth, double[,] estimate)
        {
            double[,] error = truth.Clone() as double[,];
            CDZNET.Helpers.ArrayHelper.ForEach(truth, false, (x, y) => { error[x, y] = Math.Pow(error[x, y] - estimate[x, y], 2.0); });
            return error;
        }

        Dictionary<string, double> getDirectionalErrors(double[,] truth, double[,] estimate)
        {
            Dictionary<string, double> errors = new Dictionary<string,double>();

            errors["left"] = 0.0;
            errors["top"] = 0.0;
            errors["right"] = 0.0;
            errors["bottom"] = 0.0;

            for (int x = 0; x < truth.GetLength(0); x+=4)
			{
                for (int y = 0; y < truth.GetLength(1); y++)
			    {
                    double e = 0.0;
                    for (int compo = 0; compo < 4; compo++)
                    {
                        e += Math.Pow(truth[x+compo, y] - estimate[x+compo, y], 2.0);
                    } 
                    if (x == 0)
                        errors["left"] += e;
                    if (y == 0)
                        errors["top"] += e;
                    if (x == truth.GetLength(0)-4)
                        errors["right"] += e;
                    if (y == truth.GetLength(1)-1)
                        errors["bottom"] += e;			 
			    }			 
			}

            errors["left"] /= truth.GetLength(1);
            errors["right"] /= truth.GetLength(1);
            errors["top"] /= truth.GetLength(0);
            errors["bottom"] /= truth.GetLength(0);
            return errors;
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
                    g.DrawRectangle(new Pen(Color.Black, 1), x, y, retinaSize, retinaSize);
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
