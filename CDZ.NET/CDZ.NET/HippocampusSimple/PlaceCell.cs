using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    class PlaceCell
    {
        public double[] receptiveField;
        public Dictionary<PlaceCell, double> next;
    }
}
