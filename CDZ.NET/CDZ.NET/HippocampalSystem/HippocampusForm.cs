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

namespace HippocampalSystem
{
    public partial class HippocampusForm : Form
    {
        //Environment, in this case an image
        Bitmap imageToExplore;

        //MEC
        List<IONode> mec;

        //LEC
        List<IONode> lec;
        int foveaSize = 20;

        //CA3-1
        //IONodeAdaptive DG;
        List<SignalLink> CA3Inputs;
        MMNode CA3;
        MMNode CA1;

        //Module related
        Thread networkThread;

        public HippocampusForm()
        {
            InitializeComponent();
            //--------------------------------------------------------------------------
            //-------------MEC
            mec = new List<IONode>();
            int mecAreas = 1;
            for (int i = 1; i <= mecAreas; i++)
            {
                mec.Add(
                    new IONodeGridCells
                        (
                            new Point2D(2, 1),      //Input, we cheat, this is the odometer values X, Y
                            new Point2D(10, 10),    //Number of grid cells This defines the granularity, it should vary among the MEC
                            0.1,                    //rf size
                            0.5*i                   //spacing
                        ));

                flowLayoutPanelMEC.Controls.Add(mec.Last().GetCtrl());
            }

            //--------------------------------------------------------------------------
            //--------------LEC
            lec = new List<IONode>();
            int lecAreas = 1;
            for (int i = 1; i <= lecAreas; i++)
            {
                lec.Add(
                    new IONodeAdaptiveSOM
                        (
                            new Point2D(foveaSize, foveaSize),    //Input, size of the fovea
                            new Point2D(20, 20),    //Size of the map. Defines the number of templates/filter used.
                            false)                    //USe only winner as output
                        );

                flowLayoutPanelLEC.Controls.Add(lec.Last().GetCtrl());
            }


            //--------------------------------------------------------------------------
            //--------------CA3           
            CA3Inputs = new List<SignalLink>();
            CA3 = new MMNodeSOM
                (
                    new Point2D(20, 20),    //Size of the map.
                    false                    //Use only the winner for prediction
                );

            //Add all the MEC modalities
            int counter = 0;
            foreach (IONode n in mec)
            {
                SignalLink link = new SignalLink(n.output, new Signal(n.output));
                CA3.addModality(link.to, "MEC_" + counter++); //note: n.output is cloned, not a reference             
                CA3Inputs.Add(link);
            }

            //Add the LEC modalities
            counter = 0;
            foreach (IONode n in lec)
            {
                SignalLink link = new SignalLink(n.output, new Signal(n.output));
                CA3.addModality(link.to, "LEC_" + counter++); //note: n.output is cloned, not a reference             
                CA3Inputs.Add(link);
            }
            ctrlMMNode1.attach(CA3);
        }

        void networkLoop()
        {
            Point2D position = new Point2D(0.0f, 0.0f);
            Bitmap fovea = new Bitmap(foveaSize, foveaSize);
            double dX = 0.0;
            double dY = 0.0;

            int count = 0;
            while (networkThread.IsAlive)
            {
                //Choose the next action (move along x/y)
                dX = (MathHelpers.Rand.NextDouble() * 2.0 - 1.0);
                dY = (MathHelpers.Rand.NextDouble() * 2.0 - 1.0);

                //Update the sensory input : position
                position.X += (float)dX * imageToExplore.Width;
                position.Y += (float)dY * imageToExplore.Height;
                MathHelpers.Clamp(ref position.X, 0.0f, imageToExplore.Width - foveaSize);
                MathHelpers.Clamp(ref position.Y, 0.0f, imageToExplore.Height - foveaSize);
                
                //Compute the MEC activity
                double[,] mecInput = new double[,] { { position.X }, { position.Y } };
                Parallel.ForEach(mec, mecItem =>
                {
                    mecItem.input.reality = mecInput;
                    mecItem.BottomUp();
                });

                //Compute the LEC activity
                Rectangle foveaRect = new Rectangle(new Point((int)position.X, (int)position.Y), new Size(foveaSize, foveaSize));
                fovea = imageToExplore.Clone(foveaRect, imageToExplore.PixelFormat);
                drawFoveaBorders(foveaRect);

                Parallel.ForEach(lec, lecItem =>
                {
                    lecItem.input.fromBitmap(fovea,true);
                    lecItem.BottomUp();
                });

                //Propagate the activity to CA3
                foreach(SignalLink link in CA3Inputs)
                {
                    link.FeedForward();
                }

                //Compute CA3 activity
                CA3.Cycle();

                count++;
            }
        }

        void gridCellPlot()
        {
            //StreamWriter f = new StreamWriter("testGrid.csv");
            //f.WriteLine("x,y,activity");
            //for (double i = -1.0; i < 1.0; i += 0.02)
            //{
            //    for (double j = -1.0; j < 1.0; j += 0.02)
            //    {
            //        mec.First().input.reality = new double[,] { { i }, { j } };
            //        mec.First().BottomUp();
            //        //if (mec.First().output.prediction[0, 0] > 0.5)
            //        {
            //            f.WriteLine(i + "," + j + "," + mec.First().output.prediction[0, 0]);

            //            if (pictureBox1.InvokeRequired)
            //            {
            //                this.Invoke((MethodInvoker)delegate
            //                {
            //                    Bitmap image = (Bitmap)pictureBox1.Image;
            //                    image.SetPixel((int)(image.Width * (i + 1.0) / 2.0), (int)(image.Height * (j + 1.0) / 2.0), ColorHelper.InterpolateBetween(Color.Blue,Color.Green,Color.Red, mec.First().output.prediction[0, 0]));
            //                    pictureBox1.Image = image;
            //                });
            //            }
            //            else
            //            {
            //                Bitmap image = (Bitmap)pictureBox1.Image;
            //                image.SetPixel((int)(image.Width * (i + 1.0) / 2.0), (int)(image.Height * (j + 1.0) / 2.0), ColorHelper.InterpolateBetween(Color.Blue, Color.Green, Color.Red, mec.First().output.prediction[0, 0]));
            //                pictureBox1.Image = image;
            //            }
            //        }
            //    }
            //}
            //f.Close();
        }

        private delegate void rectangleFunction(Rectangle rect);
        private void drawFoveaBorders(Rectangle foveaRect)
        {
            if (pictureBoxStimulus.InvokeRequired)
            {
                this.Invoke(new rectangleFunction(drawFoveaBorders), foveaRect);
            }
            else
            {
                pictureBoxStimulus.Image = new Bitmap(imageToExplore);
                using (Graphics g = Graphics.FromImage(pictureBoxStimulus.Image))
                {
                    g.FillRectangle(new SolidBrush(Color.Red), foveaRect);
                }
                pictureBoxStimulus.Refresh();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if( openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Load the file
                imageToExplore = new Bitmap(openFileDialog1.FileName);

                pictureBoxStimulus.Image = new Bitmap(imageToExplore);
                pictureBoxStimulus.Refresh();

                //Run the clock
                networkThread = new Thread(networkLoop);
                networkThread.Start();
            }
        }
    }
}
