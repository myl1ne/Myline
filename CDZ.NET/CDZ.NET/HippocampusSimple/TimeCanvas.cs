using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeCells
{
    public class TimeCanvas
    {
        public delegate double DistanceFunction(double[] a, double[] b);

        public double parameterLineCreationTreshold = 1.0;
        public double parameterFFLearningRate = 0.01;

        private int timeLineSize;
        private int inputSize;

        private List<TimeLine> lines = new List<TimeLine>();
        private List<TimeLine> activeLines = new List<TimeLine>();

        public DistanceFunction distanceFx;

        public TimeCanvas(int inputSize, int timeLineSize = 7, DistanceFunction distanceFunction = null)
        {
            this.inputSize = inputSize;
            this.timeLineSize = timeLineSize;
            if (distanceFunction == null)
                this.distanceFx = CDZNET.MathHelpers.distance;
            else
                this.distanceFx = distanceFunction;
        }

        public void Save(string filePath)
        {
            StreamWriter file = new StreamWriter(filePath);
            file.WriteLine(timeLineSize);

            //Write all the receptive fields on the first line    
            string lineData = "";
            foreach(TimeLine line in lines)
            {
                lineData += "(";
                for(int i=0;i<line.receptiveField.Count();i++)
                {
                    lineData += line.receptiveField[i];
                    if (i!=line.receptiveField.Count()-1)
                        lineData+=",";
                }
                lineData += ")";

                if (line != lines.Last())
                    lineData += ",";
            }
            file.WriteLine(lineData);

            //Then one line for each timeline
            foreach (TimeLine line in lines)
            {
                lineData = ""; 
                foreach(TimeCell cell in line.cells)
                {
                    lineData += "(";
                    foreach(KeyValuePair< TimeCell, double > cellNext in cell.next)
                    {
                        lineData += "(";
                        int pointToIndex = lines.IndexOf(cellNext.Key.parentLine);
                        lineData += pointToIndex + "," + cellNext.Value;
                        lineData += ")";
                        if (cell.next.Last().Key != cellNext.Key)
                            lineData += ",";
                    }
                    lineData += ")";

                    if (line != lines.Last())
                        lineData += ",";
                }
                file.WriteLine(lineData);
            }
            file.Close();
        }

        public bool Load(string filepath)
        {
            StreamReader file = new StreamReader(filepath);
            bool isFine = true;

            lines = new List<TimeLine>();
            activeLines = new List<TimeLine>();
            timeLineSize = Convert.ToInt16(file.ReadLine());
            string patternsLine = file.ReadLine();
            string[] patterns = patternsLine.Split(new string[] { "(","),(", ")" }, StringSplitOptions.RemoveEmptyEntries);
            int patternsSize = -1;
            foreach(string pattern in patterns)
            {
                string[] elements = pattern.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (patternsSize != -1 && patternsSize != elements.Count())
                    throw new Exception("Inconsistancy in the patterns size.");
                patternsSize = elements.Count();

                double[] dbPattern = new double[patternsSize];
                for (int i = 0; i < patternsSize; i++)
			    {
                    dbPattern[i] = Convert.ToDouble(elements[i]);
			    }
                lines.Add(new TimeLine(dbPattern, timeLineSize));       
            }

            //Then one line for each timeline
            int lineNumber = 0;
            foreach (TimeLine line in lines)
            {
                string lineData = "";
                try
                {
                    lineData = file.ReadLine();
                    string[] weights = lineData.Split(new string[] { "()", "((", ")),((", "))" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < line.cells.Count; i++)
                    {
                        string[] splitWeights = weights[i].Split(new string[] { "),(" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splitWeights[0] == ",")
                            continue;

                        foreach (string weight in splitWeights)
                        {
                            string[] finallySplitWeight = weight.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            int targetIndex = Convert.ToInt16(finallySplitWeight[0]);
                            double targetWeight = Convert.ToDouble(finallySplitWeight[1]);

                            TimeCell originCell = line.cells[i];
                            TimeCell targetCell = lines[targetIndex].cells[0];
                            originCell.next.Add(targetCell, targetWeight);
                            try
                            {
                                targetCell.previous[originCell] += 1;
                            }
                            catch (KeyNotFoundException)
                            {
                                targetCell.previous.Add(originCell, 1);
                            }
                        }
                    }
                    lineNumber++;
                    Console.WriteLine("Line=" + lineNumber + "/" + lines.Count);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something was wrong at " + lineNumber + "\n\t Line data is : " + lineData);
                }
            }
            file.Close();
            return isFine;
        }

        public void Reset()
        {
            foreach(TimeLine l in lines)
            {
                l.Reset();
            }
            activeLines.Clear();
        }

        public void Imprint(List<double[]> sequence, bool bidirectional = false)
        { 
            //First convert all the element to their respective timeline
            List<TimeLine> tlSequence = new List<TimeLine>();
            foreach (double[] item in sequence)
            {
                TimeLine bestLine;
                double bestScore = 0.0;
                bool hasGoodTimeline = FindBestLine(item, out bestLine, out bestScore);

                //2-Create a new timeline if necessary
                if (!hasGoodTimeline)
                {
                    bestLine = new TimeLine(item, timeLineSize);
                    lines.Add(bestLine);
                }
                tlSequence.Add(bestLine);
            }

            //Second compute the relation among elements
            for (int oldestIndex = 0; oldestIndex < tlSequence.Count; oldestIndex++)
			{
                int elementsBelow = Math.Min(timeLineSize, tlSequence.Count - oldestIndex);
                for (int level = 1; level < elementsBelow; level++)
                {
                    try
                    {
                        tlSequence[oldestIndex].cells[level].next[tlSequence[oldestIndex + level].cells[0]] += 1;
                    }
                    catch (KeyNotFoundException)
                    {
                        tlSequence[oldestIndex].cells[level].next.Add(tlSequence[oldestIndex + level].cells[0], 1.0);
                    }

                    try
                    {
                        tlSequence[oldestIndex + level].cells[0].previous[tlSequence[oldestIndex].cells[level]] += 1;
                    }
                    catch (KeyNotFoundException)
                    {
                        tlSequence[oldestIndex + level].cells[0].previous.Add(tlSequence[oldestIndex].cells[level], 1.0);
                    }
                }
			}

            if (bidirectional)
            {
                List<double[]> reversedSequence = new List<double[]>(sequence);
                reversedSequence.Reverse();
                Imprint(reversedSequence, false);
            }
        }

        public void Train(List<double[]> inputs, ref List<double[]> predictions, ref List<double> predictionsError, ref double meanPredictionsError)
        {
            Reset();
            meanPredictionsError = 0.0;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (predictions != null)
                {
                    double[] prediction = Predict();
                    double[] reality = inputs[i];
                    double itemError = distanceFx(prediction, reality);
                    predictions.Add(prediction);
                    predictionsError.Add(itemError);
                    meanPredictionsError += itemError;
                }
                Train(inputs[i]);
            }
            meanPredictionsError /= inputs.Count;
        }

        /// <summary>
        /// Teach the canvas with a new element added to the previous state.
        /// </summary>
        /// <param name="input"></param>
        public void Train(double[] input)
        {
            //1 - Find the best timeline and activate its first cell
            TimeLine bestLine = PresentInput(input, true, true);

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
            PropagateActivity();
        }

        public TimeLine PresentInput(double[] input, bool allowCreation, bool allowLearning)
        {
            //1-Find the winner timeline
            TimeLine bestLine = null;
            double bestDistance = double.PositiveInfinity;
            bool hasGoodTimeline = FindBestLine(input, out bestLine, out bestDistance);

            //2-Create a new timeline if necessary
            if (!hasGoodTimeline && allowCreation)
            {
                bestLine = new TimeLine(input, timeLineSize);
                lines.Add(bestLine);
            }
            //2-OR tune up the RF of the existing timeline
            else if (allowLearning)
            {
                for (int i = 0; i < inputSize; i++)
                {
                    double e = input[i] - bestLine.receptiveField[i];
                    bestLine.receptiveField[i] += parameterFFLearningRate * e;
                }
            }

            //3-Activate the first cell of the winner line
            bestLine.cells[0].isActive = true;

            return bestLine;
        }

        public void PropagateActivity()
        {
            foreach (TimeLine line in lines)
            {
                for (int level = line.cells.Count - 2; level >= 0; level--)
                {
                    line.cells[level + 1].isActive = line.cells[level].isActive;
                }
                line.cells[0].isActive = false;
            }
        }

        public List<TimeCell> getActiveCells()
        {
            List<TimeCell> activeCells = new List<TimeCell>();
            foreach (TimeLine line in lines)
            {
                foreach(TimeCell cell in line.cells)
                {
                    if (cell.isActive)
                        activeCells.Add(cell);
                }
            }
            return activeCells;
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
                double cDistance = distanceFx(input, line.receptiveField);
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
                predictions[line] = 1;
                TimeCell presentCell = line.cells[0];
                foreach (KeyValuePair<TimeCell,double> cell in presentCell.previous)
                {
                    if (cell.Key.isActive)
                    {
                        predictions[line] *= cell.Value;
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

        /// <summary>
        /// Generate a prediction given the current state of the network
        /// </summary>
        /// <returns> A list of prediction and their score</returns>
        public List<KeyValuePair<double[], double>> PredictAll()
        {
            List<KeyValuePair<double[], double>> predictions = new List<KeyValuePair<double[], double>>();
            foreach (TimeLine line in lines)
            {
                double score = 1;
                TimeCell presentCell = line.cells[0];
                foreach (KeyValuePair<TimeCell, double> cell in presentCell.previous)
                {
                    if (cell.Key.isActive)
                    {
                        score *= cell.Value;
                    }
                }
                predictions.Add( new KeyValuePair<double[], double>(line.receptiveField, score) );
            }
            predictions.Sort((a, b) => a.Value.CompareTo( b.Value) );
            predictions.Reverse();
            return predictions;
        }
        static char DEBUGConvert(double[] d, Dictionary<double[], char> code2char)
        {
            //find the closest
            foreach (double[] real in code2char.Keys)
            {
                if (real.SequenceEqual(d))
                    return code2char[real];
            }

            return '#';
        }
        /// <summary>
        /// Find the shortest (?) path between 2 elements
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Goal to reach</param>
        /// <param name="path">Oredered path of TimeLines to follow (take the receptive field of each for points)</param>
        /// <returns>true if a path was found, false if not</returns>
        public bool findPath(double[] start, double[] end, out List<TimeLine> path, Dictionary<double[], char> c2)
        {
            path = new List<TimeLine>();

            TimeLine bestLineStart = null;
            double bestDistanceStart = double.PositiveInfinity;
            bool hasGoodTimelineStart = FindBestLine(start, out bestLineStart, out bestDistanceStart);

            TimeLine bestLineEnd = null;
            double bestDistanceEnd = double.PositiveInfinity;
            bool hasGoodTimelineEnd = FindBestLine(end, out bestLineEnd, out bestDistanceEnd);

            //If we do not have good representation for the start or the end we give up.
            //We could also take the closest ones...
            if ( !(hasGoodTimelineStart&&hasGoodTimelineEnd) )
                return false;

            //Build the list of all the lines that can lead to this goal
            //We could sort it by their scores
            List<TimeLine> leadingLines = new List<TimeLine>();
            Dictionary<TimeLine, TimeCell> leadingCells = new Dictionary<TimeLine,TimeCell>();

            foreach(KeyValuePair<TimeCell,double> leadingCell in bestLineEnd.cells[0].previous)
            {
                leadingLines.Add(leadingCell.Key.parentLine);
                leadingCells[leadingCell.Key.parentLine] = leadingCell.Key;
            }

            List<TimeLine> initialLeadingLines = new List<TimeLine>(leadingLines);

            //Main idea: Keep the predecessors activated & propagate the current state back in time
            TimeLine currentTimeLine = bestLineStart;
            path.Add(currentTimeLine);
            bool isPathComplete = false;
            while(!isPathComplete)
            {
                TimeCell bestPredecessor = null;
                int bestStartingLevel = timeLineSize;
                char DEBUGCurrentLine = DEBUGConvert(currentTimeLine.receptiveField, c2);

                // propagate the current state back in time
                for (int currentIndexOnLine = 0; currentIndexOnLine < timeLineSize; currentIndexOnLine++)
                {
                    //Find the next cell that will lead to the shortest path to the goal
                    foreach (KeyValuePair<TimeCell, double> nextProbas in currentTimeLine.cells[currentIndexOnLine].next)
                    {
                        char DEBUGConnectedLine = DEBUGConvert(nextProbas.Key.parentLine.receptiveField, c2);
                        //If the current state leads to a goal-leading line && this leading line is the shortest so far
                        if (leadingLines.Contains(nextProbas.Key.parentLine))
                        {
                            TimeCell leadingCell = leadingCells[nextProbas.Key.parentLine];
                            if (bestPredecessor == null || bestPredecessor.level + bestStartingLevel > leadingCell.level + currentIndexOnLine)
                            {
                                bestPredecessor = leadingCell;
                                bestStartingLevel = currentIndexOnLine;
                            }
                        }
                    }
                }

                if (bestPredecessor != null)
                {
                    currentTimeLine = bestPredecessor.parentLine;
                    path.Add(currentTimeLine);

                    //THIS IS SUPER NOT OPTIMAL
                    if (initialLeadingLines.Count != leadingLines.Count)
                    {
                        leadingLines = new List<TimeLine>(initialLeadingLines);
                    }

                    //We detect the end of the path by checking that the predecessor is just 1 element behind in time
                    else if (bestPredecessor.level == 1)
                    {
                        path.Add(bestLineEnd);
                        isPathComplete = true;
                    }
                }
                else
                {
                    //Workaround the 1 element problem
                    if (leadingLines.Contains(currentTimeLine))
                    {
                        path.Add(bestLineEnd);
                        isPathComplete = true;
                        break;
                    }

                    //Problem: there is no 2 overlapping paths
                    //We expend the leading line of one level (take the leading lines leading to those leading lines)
                    List<TimeLine> completedLines = new List<TimeLine>(leadingLines);
                    foreach(TimeLine L in leadingLines)
                    {
                        foreach (KeyValuePair<TimeCell, double> leadingCell in L.cells[0].previous)
                        {
                            if (!leadingLines.Contains(leadingCell.Key.parentLine))
                            {
                                completedLines.Add(leadingCell.Key.parentLine);
                                leadingCells[leadingCell.Key.parentLine] = leadingCell.Key;
                            }
                        }
                    }
                    if (leadingLines.Count == completedLines.Count)
                    {
                        //we could not add more predecessor. There is not path.
                        break;
                    }
                    else
                    {
                        leadingLines = completedLines;
                    }
                }
            }
            return isPathComplete;
        }
    }
}
