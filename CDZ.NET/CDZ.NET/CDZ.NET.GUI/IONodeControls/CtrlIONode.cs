using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CDZNET.GUI
{
    public partial class CtrlIONode : UserControl
    {
        CDZNET.Core.IONode node;

        public CtrlIONode()
        {
            InitializeComponent();
        }

        public void attach(CDZNET.Core.IONode node)
        {
            this.node = node;
            node.onBottomUp += node_onBottomUp;
            node.onTopDown += node_onTopDown;
            node.onResized += node_onResized;

            //Just get the initial display
            node_onBottomUp(null, null);
            node_onTopDown(null, null);
            node_onResized(null, null);
        }

        void node_onTopDown(object sender, EventArgs e)
        {
            if (pictureBox3.InvokeRequired || pictureBox4.InvokeRequired)
                this.Invoke(new EventHandler(node_onTopDown), sender, e);
            else
            {
                pictureBox3.Image = node.output.toBitmap(true, Color.Black, Color.White);
                pictureBox3.Refresh();

                pictureBox4.Image = node.input.toBitmap(false, Color.Black, Color.White);
                pictureBox4.Refresh();
            }
        }

        void node_onResized(object sender, EventArgs e)
        {
            if (labelDimInput.InvokeRequired || labelDimOutput.InvokeRequired)
                this.Invoke(new EventHandler(node_onResized), sender, e);
            else
            {
                labelDimInput.Text = node.input.Width + "x" + node.input.Height;
                labelDimOutput.Text = node.output.Width + "x" + node.output.Height;
            }
        }

        void node_onBottomUp(object sender, EventArgs e)
        {
            if (pictureBox1.InvokeRequired || pictureBox2.InvokeRequired)
                this.Invoke(new EventHandler(node_onBottomUp), sender, e);
            else
            {
                pictureBox1.Image = node.input.toBitmap(true, Color.Black, Color.White);
                pictureBox1.Refresh();

                pictureBox2.Image = node.output.toBitmap(false, Color.Blue, Color.Green, Color.Red);
                pictureBox2.Refresh();
            }
        }

    }
}
