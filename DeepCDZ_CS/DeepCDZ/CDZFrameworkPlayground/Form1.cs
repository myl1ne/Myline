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

using CDZFramework.Networks;

namespace CDZFrameworkPlayground
{
    public partial class Form1 : Form
    {
        int INPUT_SIZE = 50;
        int CONSECUTIVE_PRESENTATION = 1;
        class Record
        {
            public Record() { }
            public double time;
            public double[] position = new double[3];
            public double[] velocity = new double[3];
            public double[] orientation = new double[3];
            public double[] angularVelocity = new double[3];
            public Bitmap frame;
        }
        List<Record> data = new List<Record>();


        DeepNetwork network;
        Thread networkThread;
        Random rand = new Random();
        string workingFolder;
        public Form1()
        {
            InitializeComponent();

            List<int> structure = new List<int>();
            structure.Add(10);
            structure.Add(5);
            structure.Add(1);
            //structure.Add(1);

            network = new DeepNetwork(structure, 1.0f,3,INPUT_SIZE,0.0f);
            for (int i = 0; i < network.layers.Count; i++)
            {
                CDZFramework.Gui.CtrlCDZ_2DArray arrayCtrl = new CDZFramework.Gui.CtrlCDZ_2DArray();
                arrayCtrl.Attach(network.layers[i]);
                flowLayoutPanel1.Controls.Add(arrayCtrl);
                arrayCtrl.onExpertSelection += onExpertSelection;
            }
        }

        private void onExpertSelection(object ctrl, CDZFramework.Gui.ExpertSelectionArgs args)
        {
            Bitmap bmp = network.createInputBitmap();
            network.fillRFBmp(ref bmp, args.cdz, args.expert);
            pictureBoxReceptiveField.Image = bmp;
            pictureBoxReceptiveField.Refresh();
        }

        private void buttonRunFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workingFolder = folderBrowserDialog1.SelectedPath;
                StreamReader logFile = new StreamReader(workingFolder + "/log.txt");
                
                //skip first line
                logFile.ReadLine();
                
                while(!logFile.EndOfStream)
                {
                    string line = logFile.ReadLine();
                    string[] elements = line.Split(';');
                    string frameName = elements[13];
                    if (frameName != "null")
                    {
                        Record r = new Record();
                        r.time = Convert.ToDouble(elements[0]);
                        for (int i = 0; i < 3; i++)
                        {
                            r.position[i] = Convert.ToDouble(elements[1 + i]);
                            r.velocity[i] = Convert.ToDouble(elements[1 + 3 + i]);
                            r.orientation[i] = Convert.ToDouble(elements[1 + 6 + i]);
                            r.angularVelocity[i] = Convert.ToDouble(elements[1 + 9 + i]);
                        }

                        string framePath = workingFolder + "/" + frameName;
                        Bitmap tmp = (Bitmap)Image.FromFile(@framePath, true);
                        r.frame = new Bitmap(tmp, new Size(INPUT_SIZE, INPUT_SIZE));
                        data.Add(r);
                    }
                }
                networkThread = new Thread(threadLoop);
                networkThread.Start();
            }
        }

        void threadLoop()
        {
            int dataIndex = 0;
            while(networkThread.IsAlive)
            {
                Record r = data[dataIndex];
                network.setAsInput(r.frame);
                for (int i = 0; i < CONSECUTIVE_PRESENTATION; i++)
                {
                    network.Update();
                    Bitmap presented = network.getInputPresented();
                    Bitmap prediction = network.getInputPrediction();
                    refreshGui(r.frame, presented, prediction);
                }

                if (checkBox1.Checked)
                    dataIndex = rand.Next(0, data.Count - 1);
                else
                {
                    dataIndex++;
                    if (dataIndex >= data.Count)
                        dataIndex = 0;
                }
                //Thread.Sleep(50);
            }
        }

        public delegate void DelegateRefreshGui(Bitmap bmp1, Bitmap bmp2, Bitmap bmp3);
        void refreshGui(Bitmap bmp1, Bitmap bmp2, Bitmap bmp3)
        {
            if (this.pictureBoxPresented.InvokeRequired || this.pictureBoxReal.InvokeRequired || this.pictureBoxReconstructed.InvokeRequired)
            {
                this.Invoke(new DelegateRefreshGui(refreshGui), bmp1, bmp2,bmp3);
            }
            else
            {
                pictureBoxReal.Image = bmp1;
                pictureBoxReal.Refresh();
                pictureBoxPresented.Image = bmp2;
                pictureBoxPresented.Refresh();
                pictureBoxReconstructed.Image = bmp3;
                pictureBoxReconstructed.Refresh();
            }
        }
    }
}
