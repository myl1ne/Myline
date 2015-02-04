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
    public partial class CtrlSignal : UserControl
    {
        CDZNET.Core.Signal signal = null;
        public CtrlSignal()
        {
            InitializeComponent();
        }

        public void attach(CDZNET.Core.Signal s)
        {
            signal = s;
            label3.Text = "Dimensions: " + s.Width + "x" + s.Height;
        }

        public void UpdatePrediction()
        {
            if (signal != null)
            {
                if (pictureBoxPrediction.InvokeRequired)
                    this.Invoke(new Action(UpdatePrediction));
                else
                {
                    pictureBoxPrediction.Image = signal.toBitmap(false, Color.Blue, Color.Green, Color.Red);
                    pictureBoxPrediction.Refresh();
                }
            }
        }
        public void UpdateReality()
        {
            if (signal != null)
            {
                if (pictureBoxReal.InvokeRequired)
                    this.Invoke(new Action(UpdateReality));
                else
                {
                    pictureBoxReal.Image = signal.toBitmap(true, Color.Blue, Color.Green, Color.Red);
                    pictureBoxReal.Refresh();
                }
            }
        }
    }
}
