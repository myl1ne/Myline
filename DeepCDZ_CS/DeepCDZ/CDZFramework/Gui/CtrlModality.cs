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
    public partial class CtrlModality : UserControl
    {
        CDZFramework.Core.Modality modality;

        public CtrlModality()
        {
            InitializeComponent();
        }

        public void attach(CDZFramework.Core.Modality m)
        {
            this.modality = m;
            this.modality.realValueUpdated += onUpdateReal;
            this.modality.predictedValuesUpdated += onUpdateReal;

        }

        public void onUpdateReal(object sender, EventArgs args)
        {
            if (this.pictureBoxReal.InvokeRequired)
            {
                this.Invoke(new EventHandler(onUpdateReal), sender, args);
            }
            else
            {
                this.pictureBoxReal.Image = CDZFramework.Core.Modality.toBitmap(modality.RealValues);
                this.pictureBoxReal.Refresh();
            }
        }

        public void onUpdatePredicted(object sender, EventArgs args)
        {
            if (this.pictureBoxPrediction.InvokeRequired)
            {
                this.Invoke(new EventHandler(onUpdatePredicted), sender, args);
            }
            else
            {
                this.pictureBoxPrediction.Image = CDZFramework.Core.Modality.toBitmap(modality.PredictedValues);
                this.pictureBoxPrediction.Refresh();
            }
        }
    }
}
