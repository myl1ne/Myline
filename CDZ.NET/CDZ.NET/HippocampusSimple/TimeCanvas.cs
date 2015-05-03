using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HippocampusSimple
{
    class TimeCanvas
    {
        public double parameterLineCreationTreshold = 0.25;
        public double parameterFFLearningRate = 0.01;

        private int inputSize;

        private List<TimeLine> lines = new List<TimeLine>();
        private List<TimeLine> activeLines = new List<TimeLine>();

        public TimeCanvas(int inputSize)
        {
            this.inputSize = inputSize;
        }

        public void Reset()
        {
            foreach(TimeLine l in lines)
            {
                l.Reset();
            }
            activeLines.Clear();
        }

        public double[] Update(double[] input)
        {
            double[] output = new double[inputSize];

            //1-Find the winner timeline
            TimeLine bestLine = null;
            double bestDistance = double.PositiveInfinity;
            foreach(TimeLine line in lines)
            {
                double cDistance = CDZNET.MathHelpers.distance(input, line.receptiveField);
                if (cDistance<bestDistance)
                {
                    cDistance = bestDistance;
                    bestLine = line;
                }
            }

            //2-Create a new timeline if necessary
            if (bestLine == null || bestDistance > parameterLineCreationTreshold)
            {
                bestLine = new TimeLine(input, inputSize);
                lines.Add(bestLine);
            }
            //2-OR tune up the RF of the existing timeline
            else
            {
                for(int i=0; i<inputSize; i++)
                {
                    double e = input[i] - bestLine.receptiveField[i];
                    bestLine.receptiveField[i] += parameterFFLearningRate;
                }
            }

            //3-Activate the first cell of the winner line
            bestLine.cells[0].isActive = true;

            //4-Gather all the active cells sorted by how deep they are on the timeline
            SortedList<TimeCell, int> activeCells = new SortedList<TimeCell, int>();
            foreach(TimeLine line in lines)
            {
                foreach(TimeCell cell in line.cells)
                {
                    if (cell.isActive)
                        activeCells.Add(cell, cell.level);
                }
            }

            //5-Create/Strenghten connection between those cells & the bottom cell (the first one in the list)
            for (int i = 1; i < activeCells.Count;i++ )
            {
                //Next
                TimeCell cCell = activeCells.ElementAt(i).Key;

                if (!cCell.next.ContainsKey(bestLine.cells[0]))
                {
                    cCell.next.Add(bestLine.cells[0], 1);
                }
                else
                {
                    cCell.next[bestLine.cells[0]]++;
                }
                cCell.totalEncounters++;

                //TODO LATER. This is harder to keep track ?
                //How do you manage the encounters ? Different counters for previous/next ?
                //Could that be related to this temporal direction bias ?
                ////Previous
                //if (!bestLine.cells[0].previous.ContainsKey(cCell))
                //{
                //    bestLine.cells[0].previous.Add(cCell, 1);
                //}
                //else
                //{
                //    cCell.next[bestLine.cells[0]]++;
                //}
                //cCell.totalEncounters++;
            }

            return output;
        }
    }
}
