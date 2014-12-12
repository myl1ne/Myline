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
    public partial class CtrlCDZ_ESOM : CtrlCDZ
    {
        public CtrlCDZ_ESOM()
        {
            InitializeComponent();
        }
        protected override void cdz_onConvergence(object sender, EventArgs e)
        {
            if (labelNeurons.InvokeRequired||labelConnections.InvokeRequired)
            {
                this.Invoke(new EventHandler(cdz_onConvergence), sender, e);
            }
            else
            {
                labelNeurons.Text = (cdz as CDZFramework.Core.CDZ_ESOM).neurons.Count.ToString();
                labelConnections.Text = ((cdz as CDZFramework.Core.CDZ_ESOM).ConnectionsCount/2).ToString();
                labelNeurons.Refresh();
                labelConnections.Refresh();
            }
        }
    }
}
