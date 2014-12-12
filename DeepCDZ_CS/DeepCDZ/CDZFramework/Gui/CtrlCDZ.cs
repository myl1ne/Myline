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
    public class ExpertSelectionArgs:EventArgs
    {
        public CDZFramework.Core.CDZ cdz = null;
        public int expert = -1;
    }

    public partial class CtrlCDZ : UserControl
    {
        public event EventHandler<ExpertSelectionArgs> onExpertSelected;
        
        protected CDZFramework.Core.CDZ cdz;

        public CtrlCDZ()
        {
            InitializeComponent();
        }

        public void Attach(CDZFramework.Core.CDZ cdz)
        {
            this.cdz = cdz;
            this.cdz.onConvergence += cdz_onConvergence;
        }

        protected virtual void cdz_onConvergence(object sender, EventArgs e)
        {
        }

        protected void sendEventExpertSelected(ExpertSelectionArgs args)
        {
            if (onExpertSelected != null)
            {
                onExpertSelected(this, args);
            }
        }
    }
}
