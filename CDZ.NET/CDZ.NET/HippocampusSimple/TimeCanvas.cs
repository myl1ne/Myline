using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HippocampusSimple
{
    class TimeCanvas
    {
        public double parameterLineCreationTreshold = 1.0;
        public double parameterFFLearningRate = 0.01;

        private int timeLineSize;
        private int inputSize;

        private List<TimeLine> lines = new List<TimeLine>();
        private List<TimeLine> activeLines = new List<TimeLine>();

        public TimeCanvas(int inputSize, int timeLineSize = 7)
        {
            this.inputSize = inputSize;
            this.timeLineSize = timeLineSize;
        }

        public void Reset()
        {
            foreach(TimeLine l in lines)
            {
                l.Reset();
            }
            activeLines.Clear();
        }

        public void Train(List<double[]> inputs)
        {
            Reset();

            for (int i = 0; i < inputs.Count; i++)
            {
                Train(inputs[i]);
            }
        }

        /// <summary>
        /// Teach the canvas with a new element added to the previous state.
        /// </summary>
        /// <param name="input"></param>
        public void Train(double[] input)
        {
            //1-Find the winner timeline
            TimeLine bestLine = null;
            double bestDistance = double.PositiveInfinity;
            bool hasGoodTimeline = FindBestLine(input, out bestLine, out bestDistance);

            //2-Create a new timeline if necessary
            if (!hasGoodTimeline)
            {
                bestLine = new TimeLine(input, timeLineSize);
                lines.Add(bestLine);
            }
            //2-OR tune up the RF of the existing timeline
            else
            {
                for(int i=0; i<inputSize; i++)
                {
                    double e = input[i] - bestLine.receptiveField[i];
                    bestLine.receptiveField[i] += parameterFFLearningRate * e;
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

                //Previous
                if (!bestLine.cells[0].previous.ContainsKey(cCell))
                {
                    bestLine.cells[0].previous.Add(cCell, 1);
                }
                else
                {
                    bestLine.cells[0].previous[cCell]++;
                }
            }

            //6-Make the activity propagate one step
            foreach(TimeLine line in lines)
            {
                for (int level = line.cells.Count-2; level >= 0; level--)
                {
                    line.cells[level + 1].isActive = line.cells[level].isActive;
                }
                line.cells[0].isActive = false;
            }
        }

        /// <summary>
        /// Find the best matching timeline for a given input.
        /// </summary>
        /// <param name="input">The input to match</param>
        /// <param name="bestLine">OUT the best timeline found.</param>
        /// <param name="bestDistance">OUT the best distance found.</param>
        /// <returns>True is the template is valid, false if a new one should be created.</returns>
        public bool FindBestLine(double[] input, out TimeLine bestLine, out double bestDistance)
        {
            bestLine = null;
            bestDistance = double.PositiveInfinity;
            foreach (TimeLine line in lines)
            {
                double cDistance = CDZNET.MathHelpers.distance(input, line.receptiveField);
                if (cDistance < bestDistance)
                {
                    bestDistance = cDistance;
                    bestLine = line;
                }
            }
            return !(bestLine == null || bestDistance > parameterLineCreationTreshold);
        }

        public double[] Predict(List< double[] > inputs, bool shouldCreateMissingPattern = true)
        {
            double[] output = new double[inputSize];
            Reset();

            if (inputs.Count>timeLineSize)
            {
                Console.WriteLine("WARNING: Predict() asked prediction based on more elements than the timeline length. Only the "+timeLineSize+" elements will be used.");
            }

            //1-Present the stimulus
            //The temporal order is index=0=oldest
            int startingIndex = Math.Max(0, inputs.Count-timeLineSize-1);
            for (int i = 0; i<timeLineSize; i++ )
            {
                //1-1 Find the best matching timeline
                TimeLine bestLine = null;
                double bestDistance = double.PositiveInfinity;
                bool hasGoodTimeline = FindBestLine(inputs[startingIndex+i], out bestLine, out bestDistance);
                if (!hasGoodTimeline && shouldCreateMissingPattern)
                {
                    bestLine = new TimeLine(inputs[startingIndex+i], timeLineSize);
                    lines.Add(bestLine);
                }

                //1-2 Set the cell corresponding to the right time level
                bestLine.cells[timeLineSize - i - 1].isActive = true;
            }

            //2-Generate the prediction
            return Predict();
        }

        /// <summary>
        /// Generate a prediction given the current state of the network
        /// </summary>
        /// <returns></returns>
        public double[] Predict()
        {
            TimeLine bestTimeLine = null;
            Dictionary<TimeLine, double> predictions = new Dictionary<TimeLine, double>();
            foreach(TimeLine line in lines)
            {
                predictions[line] = 0;
                TimeCell presentCell = line.cells[0];
                foreach (KeyValuePair<TimeCell,double> cell in presentCell.previous)
                {
                    if (cell.Key.isActive)
                    {
                        predictions[line] += cell.Value;
                    }
                }
                if (bestTimeLine == null || predictions[bestTimeLine]<predictions[line])
                {
                    bestTimeLine = line;
                }
            }

            if (bestTimeLine != null)
                return bestTimeLine.receptiveField;
            else
                return new double[inputSize];
        }
    }
}
