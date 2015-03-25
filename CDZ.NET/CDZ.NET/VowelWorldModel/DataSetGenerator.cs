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
using System.Diagnostics;

namespace VowelWorldModel
{
    public partial class DatasetGenerator : Form
    {
        Random rnd = new Random();
        //Parameters
        int retinaSize = 3;
        int shapeCount = 4;
        int worldWidth = 250;
        int worldHeight = 250;
        int seedsNumber = 3;
        int saccadeSize = 1;
        double orientationVariability = 0.0; //degrees
        World world;
        Dictionary<string, Bitmap> worldVisu;

        List<Dictionary<string, double[,]>> trainSet;
        List<Dictionary<string, double[,]>> testSet;

        public DatasetGenerator()
        {
            InitializeComponent();
            listBoxAlgo.DataSource = Enum.GetValues(typeof(MNNodeFactory.Model));

            //Generate the world
            world = new World(worldWidth, worldHeight, shapeCount, orientationVariability);
            world.Randomize(seedsNumber);
            getWorldVisualization();
        }

        Dictionary<string, double[,]> getRecord()
        {
            Dictionary<string, double[,]> record = new Dictionary<string, double[,]>();

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
            record["Saccade"] = new double[4, 1];
            if (dX == -saccadeSize)
            {
                record["Saccade"][0, 0] = 1;
                record["Saccade"][1, 0] = 0;
            }
            else if (dX == saccadeSize)
            {
                record["Saccade"][0, 0] = 0;
                record["Saccade"][1, 0] = 1;
            }
            else
            {
                record["Saccade"][0, 0] = 0;
                record["Saccade"][1, 0] = 0;
            }


            if (dY == -saccadeSize)
            {
                record["Saccade"][2, 0] = 1;
                record["Saccade"][3, 0] = 0;
            }
            else if (dY == saccadeSize)
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
            record["XY-t0"] = new double[2, 1] { {startX}, {startY} };
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
                    Cell startPx = world.cells[startX + i, startY + j];
                    Cell endPx = world.cells[endX + i, endY + j];

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
        #endregion

        private void buttonRdmz_Click(object sender, EventArgs e)
        {
            world.Randomize(seedsNumber);
            getWorldVisualization();
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            //Reset progress bar
            int totalSamples = Convert.ToInt16(numericUpDownSamples.Value);
            progressBarCurrentOp.Minimum = 0;
            progressBarCurrentOp.Maximum = 2*totalSamples;
            progressBarCurrentOp.Step = 1;
            progressBarCurrentOp.Value = 0;

            //Generate the dataset
            trainSet = new List<Dictionary<string, double[,]>>();
            testSet = new List<Dictionary<string, double[,]>>();
            for (int step = 0; step < totalSamples; step++)
            {
                trainSet.Add(getRecord());
                testSet.Add(getRecord());
                progressBarCurrentOp.PerformStep();
            }

            //Dump it into a file
            StreamWriter file = new StreamWriter(textBoxFileName.Text);

            //Write some metadata
            file.WriteLine("worldWidth\t" + worldWidth);
            file.WriteLine("worldHeight\t" + worldHeight);
            file.WriteLine("seedsNumber\t" + seedsNumber);
            file.WriteLine("orientationVariability\t" + orientationVariability);
            file.WriteLine("retinaSize\t" + retinaSize);
            file.WriteLine("saccadeSize\t" + saccadeSize);
            file.WriteLine();

            //write the headers
            file.Write(getMatrixHeadingS("XY-t0", 1, 1, 2));
            file.Write(getMatrixHeadingS("XY-t1", 1, 1, 2));
            file.Write(getMatrixHeadingS("Vision-t0-Color", retinaSize, retinaSize, 4));
            file.Write(getMatrixHeadingS("Vision-t0-Orientation", retinaSize, retinaSize, 2));
            file.Write(getMatrixHeadingS("Vision-t0-Shape", retinaSize, retinaSize, 4));
            file.Write(getMatrixHeadingS("Saccade", 1, 1, 4));
            file.Write(getMatrixHeadingS("Vision-t1-Color", retinaSize, retinaSize, 4));
            file.Write(getMatrixHeadingS("Vision-t1-Orientation", retinaSize, retinaSize, 2));
            file.Write(getMatrixHeadingS("Vision-t1-Shape", retinaSize, retinaSize, 4));
            file.WriteLine();

            //Write the actual data
            for (int step = 0; step < totalSamples; step++)
            {
                file.WriteLine(
                    GetString(trainSet[step]["XY-t0"]) + "," +
                    GetString(trainSet[step]["XY-t1"]) + "," +
                    GetString(trainSet[step]["Vision-t0-Color"]) + "," +
                    GetString(trainSet[step]["Vision-t0-Orientation"]) + "," +
                    GetString(trainSet[step]["Vision-t0-Shape"]) + "," +
                    GetString(trainSet[step]["Saccade"]) + "," +
                    GetString(trainSet[step]["Vision-t1-Color"]) + "," +
                    GetString(trainSet[step]["Vision-t1-Orientation"]) + "," +
                    GetString(trainSet[step]["Vision-t1-Shape"])
                    );

                progressBarCurrentOp.PerformStep();
            }
            file.Close();
            MessageBox.Show("Dataset generated : " + totalSamples + " records");
            buttonTrain.Enabled = true;
            buttonTest.Enabled = true;
        }

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            //Reset progress bar
            int MAXIMUM_EPOCH = 5000;
            progressBarCurrentOp.Minimum = 0;
            progressBarCurrentOp.Maximum = MAXIMUM_EPOCH;
            progressBarCurrentOp.Step = 1;
            progressBarCurrentOp.Value = 0;

            MNNodeFactory.Model selectedModel;
            Enum.TryParse<MNNodeFactory.Model>(listBoxAlgo.SelectedItem.ToString(), out selectedModel);
            MMNode network = MNNodeFactory.obtain(selectedModel);

            network.onEpoch += network_onEpoch;
            //network.addModality( new Signal(2,1), "XY-t0");
            //network.addModality( new Signal(2,1), "XY-t1");
            network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t0-Color");
            network.addModality(new Signal(retinaSize * 2, retinaSize), "Vision-t0-Orientation");
            network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t0-Shape");
            network.addModality(new Signal(4, 1), "Saccade");
            network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t1-Color");
            network.addModality(new Signal(retinaSize * 2, retinaSize), "Vision-t1-Orientation");
            network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t1-Shape");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            int iterations = network.Batch(trainSet, MAXIMUM_EPOCH, 0.01);
            watch.Stop();
            MessageBox.Show("Batch trained operated in " + watch.Elapsed + " over " + iterations + " iterations.");
        }

        void network_onEpoch(int currentEpoch, int maximumEpoch, Dictionary<Signal, double> modalitiesMSE, double MSE)
        {
            progressBarCurrentOp.PerformStep();
            labelError.Text = MSE.ToString();
            labelError.Refresh();
        }
    }
}
