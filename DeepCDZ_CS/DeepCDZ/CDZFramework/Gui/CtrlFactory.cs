using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework.Gui
{
    public static class CtrlFactory
    {
        public static CtrlCDZ getCtrl(CDZFramework.Core.CDZ cdz)
        {
            Type t = cdz.GetType();
            CtrlCDZ ctrl;
            if (t == typeof(CDZFramework.Core.CDZ_DSOM))
                ctrl = new CtrlCDZ_DSOM();
            else if (t == typeof(CDZFramework.Core.CDZ_ESOM))
                ctrl = new CtrlCDZ_ESOM();
            else
                ctrl = new CtrlCDZ();

            ctrl.Attach(cdz);
            return ctrl;
        }
    }
}
