using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.GUI
{
    public static class IONodeExtensions
    {
        public static GUI.CtrlIONode GetCtrl(this CDZNET.Core.IONode node)
        {
            GUI.CtrlIONode ctrl = new GUI.CtrlIONode();
            ctrl.attach(node);
            return ctrl;
        }
    }
}
