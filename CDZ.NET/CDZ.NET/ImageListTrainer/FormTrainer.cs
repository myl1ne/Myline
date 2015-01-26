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

using CDZNET;
using CDZNET.GUI;

namespace CDZNET.Applications.ImageListTrainer
{
    public partial class FormTrainer : Form
    {
        Core.IONodeDeepNet network;

        Thread networkThread;
        Random rand = new Random();
        string workingFolder;

        List<LabelledImage> trainingSet = new List<LabelledImage>();

        public FormTrainer()
        {
            InitializeComponent();

            network = new Core.IONodeDeepNet();


            //network.pushLayer(
            //        new Core.IONodeConvolution( 
            //            new Point2D(100,100), //Input dimensions
            //            new Core.IONodeKernel(new double[,] { //Type of filter used and its size
            //                {0.0, 1.0, 0.0}, 
            //                {1.0,-4.0, 1.0},
            //                {0.0, 1.0, 0.0} }), 
            //            new Point2D(1,1))); //Step size

            //network.pushLayer(
            //    new Core.IONodeConvolution(
            //        new Point2D(28, 28), //Input dimensions
            //        new Core.IONodeAdaptiveSOM(
            //            new Point2D(4, 4), //Size of the input (filter)
            //            new Point2D(20, 20), //Size of the SOM
            //            true //Use only winner or whole population
            //            ),
            //    new Point2D(4,4))); //Step size

            //network.pushLayer(
            //            new Core.IONodeAdaptiveSOM(
            //            network.output.Size, //Size of the input (filter)
            //            new Point2D(20, 20), //Size of the SOM
            //            false //Use only winner or whole population
            //            ));

            network.pushLayer(
                        new Core.IONodeAdaptiveSOM(
                        new Point2D(28, 28), //Size of the input (filter)
                        new Point2D(30, 30), //Size of the SOM
                        false //Use only winner or whole population
                        ));
            //network.pushLayer(
            //    new Core.IONodeConvolution( 
            //        new Point(320,240), //Input dimensions
            //        new Core.IONodeMax(new Point(5,5)), //Type of filter used and its size
            //        new Point(1,1))); //Step size

            //Add the controls
            foreach(Core.IONode n in network.Layers)
            {
                flowLayoutPanel1.Controls.Add(n.GetCtrl());
            }

            networkThread = new Thread(networkThreadLoop);
        }

        private void networkThreadLoop()
        {
            int dataIndex = 0;
            while (networkThread.IsAlive)
            {
                if (!checkBoxPause.Checked)
                {
                    Bitmap bmp = trainingSet[dataIndex].image;
                    network.input.fromBitmap(bmp, true);
                    network.BottomUp();
                    network.TopDown();

                    bool randomOrder = false;
                    if (randomOrder)
                        dataIndex = rand.Next(0, trainingSet.Count - 1);
                    else
                    {
                        dataIndex++;
                        if (dataIndex >= trainingSet.Count)
                            dataIndex = 0;
                    }
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workingFolder = folderBrowserDialog1.SelectedPath;
                foreach(string file in System.IO.Directory.EnumerateFiles(workingFolder, "*.png"))
                {
                    LabelledImage img = new LabelledImage();
                    img.image = (Bitmap)Image.FromFile(@file, true);
                    trainingSet.Add(img);
                }
                networkThread.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Application.StartupPath;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workingFolder = folderBrowserDialog1.SelectedPath;
                trainingSet = DatabaseMNIST.loadData(workingFolder + "\\train-images.idx3-ubyte", workingFolder + "\\train-labels.idx1-ubyte");
                networkThread.Start();
            }
        }
    }
}
