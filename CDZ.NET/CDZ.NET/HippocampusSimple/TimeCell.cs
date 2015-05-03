using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HippocampusSimple
{
    class TimeCell
    {
        public int level { public get; private set; }
        public int totalEncounters = 0;

        public bool isActive;
        public Dictionary<TimeCell, double> next { get; set; }
        public Dictionary<TimeCell, double> previous { get; set; }
        
        public TimeCell(int level)
        {
            this.level = level;
        }
    }
}
