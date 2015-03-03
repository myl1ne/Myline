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

namespace VowelWorldModel
{
    public partial class MainWindow : Form
    {
        //Parameters
        int retinaSize = 5;
        int shapeCount = 4;
        int worldWidth = 100;
        int worldHeight = 100;
        int seedsNumber = 5;
        double orientationVariability = 10.0; //degrees
        World world;
        Bitmap worldVisu;

        //Network structures
        //1-Inputs
        CDZNET.Core.Signal LEC_Color; //Visual matrix
        CDZNET.Core.Signal LEC_Orientation; //Visual matrix
        CDZNET.Core.Signal LEC_Shape; //Visual matrix
        CDZNET.Core.Signal MEC;//Proprioception/Grid Cells

        //2-Areas
        CDZNET.Core.MMNode CA3; //Here you specify which algo to be used

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
            LEC_Color = new CDZNET.Core.Signal(retinaSize, retinaSize); //Visual matrix
            LEC_Orientation = new CDZNET.Core.Signal(retinaSize, retinaSize); //Visual matrix
            LEC_Shape = new CDZNET.Core.Signal(retinaSize, retinaSize); //Visual matrix
            MEC = new CDZNET.Core.Signal(2, 1); //Proprioception/Grid Cells

            //2-Areas
            CA3 = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(20, 20), false); //Here you specify which algo to be used
            CA3.addModality(LEC_Color, "Color");
            CA3.addModality(LEC_Orientation, "Orientation");
            CA3.addModality(LEC_Shape, "Shape");
            CA3.addModality(MEC, "XY");

            //Gui purpose
            ctrlCA3.attach(CA3);

            mainLoopThread = new Thread(mainLoop);
            mainLoopThread.Start();
        }

        public void mainLoop()
        {
            //Explore the world
            Random rnd = new Random();
            //int explorationSteps = 1000;
            //for (int step = 0; step < explorationSteps; step++)
            while (mainLoopThread.IsAlive)
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
                        LEC_Color.reality[retinaSize / 2 + i, retinaSize / 2 + j] = px.colorValue;
                        LEC_Orientation.reality[retinaSize / 2 + i, retinaSize / 2 + j] = Math.Abs(px.orientation % 180.0) / 180.0;
                        LEC_Shape.reality[retinaSize / 2 + i, retinaSize / 2 + j] = px.shape / (double)shapeCount; //  !!! This encoding is bad because it defines intrinsic shape distance
                    }
                }

                //-----------------
                //Run a cycle on CA3
                CA3.Converge();
                CA3.Diverge();
            }
        }



        //---------------------VISUALIZATION------------------
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
    }
}
