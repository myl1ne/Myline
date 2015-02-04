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
    public partial class CtrlMMNode : UserControl
    {
        CDZNET.Core.MMNode node;

        public CtrlMMNode()
        {
            InitializeComponent();
        }

        public void attach(CDZNET.Core.MMNode node)
        {
            this.node = node;
            foreach(CDZNET.Core.Signal s in node.modalities)
            {
                CtrlSignal ctrl = new CtrlSignal();
                ctrl.attach(s);
                flowLayoutPanel1.Controls.Add(ctrl);
            }
            node.onConvergence += node_onConvergence;
            node.onDivergence += node_onDivergence;
        }

        void node_onDivergence(object sender, EventArgs e)
        {
            foreach (Control c in flowLayoutPanel1.Controls)
            {
                if (c is CtrlSignal)
                    (c as CtrlSignal).UpdatePrediction();
            }
        }

        void node_onConvergence(object sender, EventArgs e)
        {

            if (pictureBoxMapActivity.InvokeRequired)
            {
                this.Invoke(new EventHandler(node_onConvergence), sender, e);
                return;
            }

            pictureBoxMapActivity.Image = node.output.toBitmap(false, Color.Blue, Color.Green, Color.Red);
            pictureBoxMapActivity.Refresh();

            foreach (Control c in flowLayoutPanel1.Controls)
            {
                if (c is CtrlSignal)
                    (c as CtrlSignal).UpdateReality();
            }     
        }

    }
}
