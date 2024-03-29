﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    public class TimeCell:IComparable
    {
        public int level { get; private set; }
        public int totalEncounters = 0;
        public TimeLine parentLine {get;private set;}
        public bool isActive;
        public Dictionary<TimeCell, double> next = new Dictionary<TimeCell, double>();
        public Dictionary<TimeCell, double> previous = new Dictionary<TimeCell, double>();
        
        public TimeCell(int level, TimeLine parentLine)
        {
            this.level = level;
            this.parentLine = parentLine;
        }

        public int CompareTo(object obj)
        {
            if (obj is TimeCell)
            {
                TimeCell other = obj as TimeCell;
                return this.level.CompareTo(other.level);
            }
            return 0;
        }
    }
}
