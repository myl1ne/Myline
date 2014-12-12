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
    public partial class CtrlCDZ_2DArray : UserControl
    {
        public event EventHandler<ExpertSelectionArgs> onExpertSelection;
        public CtrlCDZ_2DArray()
        {
            InitializeComponent();
        }

        public void Attach(CDZFramework.Core.CDZ[,] grid)
        {
            //Fill it
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                FlowLayoutPanel line = new FlowLayoutPanel();
                line.FlowDirection = FlowDirection.LeftToRight;
                line.AutoSize = true;

                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    CtrlCDZ ctrl = CDZFramework.Gui.CtrlFactory.getCtrl(grid[x, y]);
                    line.Controls.Add(ctrl);
                    ctrl.onExpertSelected += propagateExpertSelection;
                }
                flowLayoutPanel1.Controls.Add(line);
            }
        }
        
        private void propagateExpertSelection(object ctrlCDZ, ExpertSelectionArgs args)
        {
            if (onExpertSelection != null)
            {
                onExpertSelection(ctrlCDZ, args);
            }
        }
    }
}
