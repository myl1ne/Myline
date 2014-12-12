using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CDZFramework.Gui
{
    public partial class CtrlCDZ_DSOM : CtrlCDZ
    {
 
        public CtrlCDZ_DSOM()
        {
            InitializeComponent();
        }

        protected override void cdz_onConvergenceChildren(object sender, EventArgs e)
        {
            if (pictureBox1.InvokeRequired)
            {
                this.Invoke(new EventHandler(cdz_onConvergenceChildren), sender, e);
            }
            else
            {
                pictureBox1.Image = (cdz as CDZFramework.Core.CDZ_DSOM).asBitmap(true);
                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            Coordinates2D neuronCoo;
            neuronCoo.x = (cdz as CDZFramework.Core.CDZ_DSOM).Width * coordinates.X / (1+pictureBox1.Width);
            neuronCoo.y = (cdz as CDZFramework.Core.CDZ_DSOM).Height * coordinates.Y / (1+pictureBox1.Height);
            //MessageBox.Show("You clicked on:" + neuronCoo.x + " - " + neuronCoo.y);
            ExpertSelectionArgs args = new ExpertSelectionArgs();
            args.cdz = this.cdz;
            args.expert = this.cdz.getExpertIndex((cdz as CDZFramework.Core.CDZ_DSOM).Neuron((int)Math.Floor(neuronCoo.x), (int)Math.Floor(neuronCoo.y)));
            sendEventExpertSelected(args);
        }
    }
}
