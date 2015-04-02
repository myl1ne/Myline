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
using CDZNET.Helpers;

namespace VowelWorldModel
{
    public partial class DatasetGenerator : Form
    {
        Random rnd = new Random();

        bool EXTENSIVE_LOG = false;

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

        public DatasetGenerator()
        {
            InitializeComponent();
            listBoxAlgo.DataSource = Enum.GetValues(typeof(MMNodeFactory.Model));

            //Generate the world
            world = new World(worldWidth, worldHeight, shapeCount, orientationVariability);
            world.Randomize(seedsNumber);
            getWorldVisualization();
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
            //MessageBox.Show("Dataset generated : " + totalSamples + " records");
            //buttonTrain.Enabled = true;
            //buttonTest.Enabled = true;
        }

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            //Generate the sets
            int sampleCount = 2000;
            Dictionary<string, List<Dictionary<string, double[,]>>> setsToEvaluate = world.generateSaccadeDatasets(sampleCount, retinaSize, saccadeSize);

            //Choose the models to test
            List<MMNodeFactory.Model> modelsToTest = new List<MMNodeFactory.Model>()
            {
                MMNodeFactory.Model.DeepBelief,
                //MNNodeFactory.Model.LUT,
                MMNodeFactory.Model.AFSOM,
                //MNNodeFactory.Model.SOM,
                ////MNNodeFactory.Model.MWSOM,
                //MMNodeFactory.Model.MLP
            };

            //Vary the size of the trainingset
            for (int itemToTrainOn = 100; itemToTrainOn <= setsToEvaluate["train"].Count; itemToTrainOn += (itemToTrainOn==100)?400:500)
            {   
                //Create
                Dictionary<string, MMNode> models = createModels(modelsToTest);

                //Train
                setsToEvaluate["usedForTraining"] = setsToEvaluate["train"].GetRange(0, itemToTrainOn);
                trainModels(models, setsToEvaluate["usedForTraining"]);

                //Test
                evaluateOnSets(models, setsToEvaluate, "testLog_"+ textBoxFileName.Text);
            }
            MessageBox.Show("Done !");
        }

        void network_onEpoch(int currentEpoch, int maximumEpoch, Dictionary<Signal, double> modalitiesMSE, double MSE)
        {
            progressBarCurrentOp.PerformStep();
            labelError.Text = MSE.ToString();
            labelError.Refresh();
        }

        void dumpDatasetToFile(List<Dictionary<string, double[,]>> dataset, string fileName)
        {
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
            for (int step = 0; step < dataset.Count; step++)
            {
                file.WriteLine(
                    GetString(dataset[step]["XY-t0"]) + "," +
                    GetString(dataset[step]["XY-t1"]) + "," +
                    GetString(dataset[step]["Vision-t0-Color"]) + "," +
                    GetString(dataset[step]["Vision-t0-Orientation"]) + "," +
                    GetString(dataset[step]["Vision-t0-Shape"]) + "," +
                    GetString(dataset[step]["Saccade"]) + "," +
                    GetString(dataset[step]["Vision-t1-Color"]) + "," +
                    GetString(dataset[step]["Vision-t1-Orientation"]) + "," +
                    GetString(dataset[step]["Vision-t1-Shape"])
                    );

                progressBarCurrentOp.PerformStep();
            }
            file.Close();
        }

        Dictionary<string, MMNode> createModels(List<MMNodeFactory.Model> modelsToTrain)
        {
            Dictionary<string, MMNode> models = new Dictionary<string, MMNode>();

            for (int neuronsCount = 10; neuronsCount < 100; neuronsCount += 20)
            {
                foreach (MMNodeFactory.Model selectedModel in modelsToTrain)
                {
                    //Create the model
                    //MNNodeFactory.Model selectedModel;
                    //Enum.TryParse<MNNodeFactory.Model>(uncastedMdl.ToString(), out selectedModel);
                    models[selectedModel.ToString() + "_" + neuronsCount] = MMNodeFactory.obtain(selectedModel, neuronsCount);

                    MMNode network = models[selectedModel.ToString() + "_" + neuronsCount];
                    network.onEpoch += network_onEpoch;
                    //network.addModality( new Signal(2,1), "XY-t0");
                    //network.addModality( new Signal(2,1), "XY-t1");
                    network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t0-Color");
                    network.addModality(new Signal(retinaSize * 2, retinaSize), "Vision-t0-Orientation");
                    //network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t0-Shape");
                    network.addModality(new Signal(4, 1), "Saccade");
                    network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t1-Color");
                    network.addModality(new Signal(retinaSize * 2, retinaSize), "Vision-t1-Orientation");
                    //network.addModality(new Signal(retinaSize * 4, retinaSize), "Vision-t1-Shape");

                    //Apply a treshold function on the modalities
                    network.onDivergence += network_onDivergence;
                }
            }
            return models;
        }

        /// <summary>
        /// Apply a treshold function on the modalities after divergence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void network_onDivergence(object sender, EventArgs e)
        {
            foreach(Signal s in (sender as MMNode).modalities)
            {
                ArrayHelper.ForEach(s.prediction, true, (x, y) =>
                    {
                        s.prediction[x, y] = (s.prediction[x, y] >= 0.5) ? 1.0 : 0.0;
                    });
            }
        }

        int countWrongPixels(string modalityName, double[,] truth, double[,] estimation)
        {
            int components = 4;
            if (modalityName.Contains("Orientation"))
                components = 2;
            else if (modalityName.Contains("Color"))
                components = 4;
            else
                throw new Exception("Trying to count some non visual stuff ?");

            int error = 0;
            for (int i = 0; i < retinaSize; i++)
            {
                for (int j = 0; j < retinaSize; j++)
                {
                    for (int comp = 0; comp < components; comp++)
                    {
                        if (truth[i*components,j] != estimation[i*components,j])
                        {
                            error++; break;
                        }
                    }
                }
            }
            return error;
        }

        void trainModels(Dictionary<string, MMNode> modelsToTrain, List<Dictionary<string, double[,]>> trainingSet)
        {
            Console.WriteLine();
            Console.WriteLine("---New training round ---");
            foreach (KeyValuePair<string, MMNode> model in modelsToTrain)
            {
                Console.WriteLine(DateTime.Now + "\t" + "Start training " + model.Key + " on a set of size " + trainingSet.Count);

                //Reset progress bar
                int MAXIMUM_EPOCH = 5000;
                progressBarCurrentOp.Minimum = 0;
                progressBarCurrentOp.Maximum = MAXIMUM_EPOCH;
                progressBarCurrentOp.Step = 1;
                progressBarCurrentOp.Value = 0; ;

                //Run a batch training
                Stopwatch watch = new Stopwatch();
                watch.Start();
                int iterations = model.Value.Batch(trainingSet, MAXIMUM_EPOCH, 0.1);
                watch.Stop();
                Console.WriteLine(DateTime.Now + "\t" + "training operated in " + watch.Elapsed + " with iterations= " + iterations);
            }
        }

        bool hasWrittenHeaders = false;
        void evaluateOnSets(Dictionary<string, MMNode> models, Dictionary<string, List<Dictionary<string, double[,]>>> sets, string logFile)
        {
            Console.WriteLine(DateTime.Now + "\t" + "Starting to test log.");

            //Dump it into a file
            StreamWriter file = new StreamWriter(logFile, hasWrittenHeaders);

            //Write some metadata
            if (!hasWrittenHeaders)
            {
                file.WriteLine("worldWidth\t" + worldWidth);
                file.WriteLine("worldHeight\t" + worldHeight);
                file.WriteLine("seedsNumber\t" + seedsNumber);
                file.WriteLine("orientationVariability\t" + orientationVariability);
                file.WriteLine("retinaSize\t" + retinaSize);
                file.WriteLine("saccadeSize\t" + saccadeSize);
                file.WriteLine();

                //write the headers
                file.Write("Model,");
                file.Write("SetName,");
                file.Write("InvertedBits,");
                file.Write("TrainingSetSize,");
                file.Write("AllModSumError,");
                if (EXTENSIVE_LOG)
                {
                    file.Write(getMatrixHeadingS("XY-t0", 1, 1, 2));
                    file.Write(getMatrixHeadingS("XY-t1", 1, 1, 2));
                    file.Write(getMatrixHeadingS("Vision-t0-Color", retinaSize, retinaSize, 4));
                    file.Write(getMatrixHeadingS("Vision-t0-Orientation", retinaSize, retinaSize, 2));
                    file.Write(getMatrixHeadingS("Vision-t0-Shape", retinaSize, retinaSize, 4));
                    file.Write(getMatrixHeadingS("Saccade", 1, 1, 4));
                    file.Write(getMatrixHeadingS("Vision-t1-Color", retinaSize, retinaSize, 4));
                    file.Write(getMatrixHeadingS("Vision-t1-Orientation", retinaSize, retinaSize, 2));
                    file.Write(getMatrixHeadingS("Vision-t1-Shape", retinaSize, retinaSize, 4));
                }
                foreach (Signal mod in models.First().Value.modalities)
                {
                    string modName = models.First().Value.labelsModalities[mod];
                    file.Write(getMatrixHeadingS("corruption_" +modName , 1, 1, 1));
                    if (EXTENSIVE_LOG)
                    {
                        file.Write(getMatrixHeadingS("reality_" + modName, 1, 1, mod.Width * mod.Height));
                        file.Write(getMatrixHeadingS("prediction_" + modName, 1, 1, mod.Width * mod.Height));
                    }
                    file.Write(getMatrixHeadingS("originalMaxError_" + modName, 1, 1, 1));
                    file.Write(getMatrixHeadingS("corruptedMaxError_" + modName, 1, 1, 1));
                    file.Write(getMatrixHeadingS("originalSumError_" + modName, 1, 1, 1));
                    file.Write(getMatrixHeadingS("corruptedSumError_" + modName, 1, 1, 1));
                    if (modName.Contains("Vision"))
                    {
                        file.Write("wrongPixels_" + modName+",");
                    }
                }
                file.WriteLine();
                hasWrittenHeaders = true;
            }

            //Start the test
            foreach (string modelName in models.Keys)
            {
                Console.Write("Testing" + modelName + " ...");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                MMNode network = models[modelName];

                network.learningLocked = true;
                foreach (string setName in sets.Keys)
                {
                    //We have "usedForTraining"
                    if (setName == "train")
                        continue;

                    List<Dictionary<string, double[,]>> set = sets[setName];
                    //Test with different level of noise
                    for (double bitShiftProb = 0.0; bitShiftProb <= 1.0; bitShiftProb += 0.5)
                    {
                        foreach (Dictionary<string, double[,]> sample in set)
                        {
                            //Set the modalities
                            int invertedBits = 0;
                            Dictionary<Signal, double> modalityCorruption = new Dictionary<Signal, double>();
                            foreach (Signal s in network.modalities)
                            {
                                modalityCorruption[s] = 0.0;

                                s.reality = sample[network.labelsModalities[s]].Clone() as double[,];

                                //Corrupt the signal
                                if (network.labelsModalities[s].Contains("t1"))
                                {
                                    modalityCorruption[s] = bitShiftProb;
                                    //1-----------------Toggle the bit
                                    //ArrayHelper.ForEach(s.reality, false, (x, y) =>
                                    //{
                                    //    if (MathHelpers.Rand.NextDouble() < bitShiftProb)
                                    //    {
                                    //        s.reality[x, y] = Math.Abs(s.reality[x, y] - 1.0);
                                    //        invertedBits++;
                                    //    }
                                    //});

                                    //2-----------------Set bit to 0.5
                                    ArrayHelper.ForEach(s.reality, false, (x, y) =>
                                    {
                                        if (MathHelpers.Rand.NextDouble() < bitShiftProb)
                                        {
                                            s.reality[x, y] = 0.5;
                                            invertedBits++;
                                        }
                                    });
                                }
                            }

                            network.Converge();
                            network.Diverge();

                            double globalError = 0.0;
                            foreach (Signal s in network.modalities)
                            {
                                globalError += s.ComputeSumAbsoluteError();
                            }

                            //Dump the info
                            string line = "";
                            line +=
                                modelName + "," +
                                setName + "," +
                                invertedBits + "," +
                                sets["usedForTraining"].Count + "," +
                                globalError + ",";

                            if (EXTENSIVE_LOG)
                                line +=
                                GetString(sample["XY-t0"]) + "," +
                                GetString(sample["XY-t1"]) + "," +
                                GetString(sample["Vision-t0-Color"]) + "," +
                                GetString(sample["Vision-t0-Orientation"]) + "," +
                                GetString(sample["Vision-t0-Shape"]) + "," +
                                GetString(sample["Saccade"]) + "," +
                                GetString(sample["Vision-t1-Color"]) + "," +
                                GetString(sample["Vision-t1-Orientation"]) + "," +
                                GetString(sample["Vision-t1-Shape"]) + ",";
                                

                            foreach (Signal s in network.modalities)
                            {
                                line += modalityCorruption[s] + ",";
                                if (EXTENSIVE_LOG)
                                    line +=
                                    GetString(s.reality) + "," +
                                    GetString(s.prediction) + ",";

                                line +=
                                    MathHelpers.maximumAbsoluteDistance(s.prediction, sample[network.labelsModalities[s]]) + "," +
                                    MathHelpers.maximumAbsoluteDistance(s.prediction, s.reality) + "," +
                                    MathHelpers.sumAbsoluteDistance(s.prediction, sample[network.labelsModalities[s]]) + "," +
                                    MathHelpers.sumAbsoluteDistance(s.prediction, s.reality) + ",";

                                string modName = network.labelsModalities[s];
                                if (modName.Contains("Vision"))
                                {
                                    line += countWrongPixels(modName,s.prediction, sample[network.labelsModalities[s]]).ToString() + ",";
                                }
                            }

                            file.WriteLine(line);
                        }
                    }
                }
                file.Flush();
                watch.Stop();
                Console.WriteLine("Done {0}", watch.Elapsed);
            }
            file.Close();
            Console.WriteLine(DateTime.Now + "\t" + "Test log written.");
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //EvaluateOnSets(testSet, "test_"+textBoxFileName.Text);
            MessageBox.Show("This is a dummy button...");
        }
    }
}
