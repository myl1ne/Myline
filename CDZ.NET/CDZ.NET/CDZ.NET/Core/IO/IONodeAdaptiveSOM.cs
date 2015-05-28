using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// An IO node that adaptes itself after each BottomUp or TopDown call.
    /// Uses the SOM algorithm. 
    /// The dimension of the output is either 2 (position of the winner in [0,1] or the dimension of the 
    /// map itself.
    /// </summary>
    public class IONodeAdaptiveSOM:IONodeAdaptive
    {
        //Parameters
        public double learningRadius;
        public double learningRate;
        public double elasticity;

        private delegate void Operation2D(int x, int y);
        private delegate void Operation4D(int x1, int y1, int x2, int y2);
        private delegate void DoubleOperation(double a);

        double[,] activity;
        double[, , ,] weights;

        double[,] previousOutput;
        double[,] recurrentActivity;
        double[, , ,] recurrentWeights;

        Point2D winner;
        double winnerActivity;
        Point2D looser;
        double looserActivity;
        bool useWinnerPositionAsOutput;

        /// <summary>
        /// Constructs a new SOM node.
        /// </summary>
        /// <param name="inputDim">The dimension of the input</param>
        /// <param name="mapSize">The dimension of the map</param>
        /// <param name="useOnlyWinnerAsOutput">Should only the position of the winner be used as an output. False means the whole map activity will be used.</param>
        public IONodeAdaptiveSOM(Point2D inputDim, Point2D mapSize, bool useOnlyWinnerAsOutput = true, bool isRecurrent = false)
            :base(inputDim, (useOnlyWinnerAsOutput)?new Point2D(2,1):mapSize)
        {
            activity = new double[(int)mapSize.X, (int)mapSize.Y];
            weights = new double[(int)inputDim.X, (int)inputDim.Y, (int)mapSize.X, (int)mapSize.Y];

            if (isRecurrent)
            {
                recurrentActivity = new double[(int)mapSize.X, (int)mapSize.Y];
                recurrentWeights = new double[output.Width, output.Height, (int)mapSize.X, (int)mapSize.Y];
                previousOutput = new double[output.Width, output.Height];
            }
            else
            {
                recurrentWeights = null;
                previousOutput = null;
            }

            useWinnerPositionAsOutput = useOnlyWinnerAsOutput;
            winner = new Point2D(0, 0);
            looser = new Point2D(0, 0);

            learningRadius = (1.0 / 4.0) * (activity.GetLength(0) + activity.GetLength(1)) / 2.0;
            learningRate = 0.1;
            elasticity = 4.0;

            //Assign the weights randomly
            ForEach(weights, false, (x1, y1, x2, y2) => { weights[x1, y1, x2, y2] = MathHelpers.Rand.NextDouble(); });
            if (recurrentWeights!=null)
                ForEach(recurrentWeights, false, (x1, y1, x2, y2) => { recurrentWeights[x1, y1, x2, y2] = MathHelpers.Rand.NextDouble(); });
        }

        protected override void bottomUpAdaptation()
        {
            bool USE_DSOM = true;
            //Standard SOM
            double squaredRadius2 = 2 * learningRadius * learningRadius;
           
            //DSOM
            double winnerError = 1.0 - winnerActivity;
            double inversedSquaredElasticity = -(1 / (elasticity*elasticity));

            ForEach(weights,true, (x1, y1, x2, y2) =>
            {
                double distanceToWinner = MathHelpers.distance(x2, y2, winner.X, winner.Y, Connectivity.torus, activity.GetLength(0), activity.GetLength(1));
                double factor = Math.Exp(-(double)(distanceToWinner) / squaredRadius2);
                if (USE_DSOM)
                    factor = learningRate * Math.Exp(inversedSquaredElasticity * (distanceToWinner / winnerError));
                else
                    factor = learningRate * Math.Exp(-(double)(distanceToWinner) / squaredRadius2);

                weights[x1, y1, x2, y2] += factor * (input.reality[x1, y1] - weights[x1, y1, x2, y2]);
            });

            //if (recurrentWeights != null)
            //{
            //    ForEach(recurrentWeights, true, (x1, y1, x2, y2) =>
            //    {
            //        double distanceToWinner = MathHelpers.distance(x2, y2, winner.X, winner.Y, Connectivity.torus, activity.GetLength(0), activity.GetLength(1));
            //        double factor = Math.Exp(-(double)(distanceToWinner) / squaredRadius2);
            //        bool USE_DSOM = true;
            //        if (USE_DSOM)
            //            factor = learningRate * Math.Exp(inversedSquaredElasticity * (distanceToWinner / winnerError));
            //        else
            //            factor = learningRate * Math.Exp(-(double)(distanceToWinner) / squaredRadius2);

            //        recurrentWeights[x1, y1, x2, y2] += factor * (previousOutput[x1, y1] - recurrentWeights[x1, y1, x2, y2]);
            //    });
            //}
        }

        protected override void topDownAdaptation()
        {
            //Here could we self organize as the other direction ?
        }

        protected override void bottomUp()
        {
            //Zero the activity
            ForEach(activity, true, (i, j) => { activity[i, j] = 0.0; });

            //Compute the activity
            ForEach(weights, false, (x1, y1, x2, y2) =>
                {
                    activity[x2, y2] += 1.0 - Math.Abs(weights[x1, y1, x2, y2] - input.reality[x1, y1]);
                }
                );
            
            if (recurrentWeights != null)
            {
                //Copy the past output (for training)
                if (recurrentWeights != null)
                {
                    Array.Copy(output.prediction, previousOutput, previousOutput.Length);
                }

                ForEach(recurrentActivity, true, (i, j) => { recurrentActivity[i, j] = 0.0; });
                ForEach(recurrentWeights, false, (x1, y1, x2, y2) =>
                {
                    recurrentActivity[x2, y2] += 1.0 - Math.Abs(recurrentWeights[x1, y1, x2, y2] - previousOutput[x1, y1]);
                }
                );
            }

            //Scale in [0,1] && find winner
            winner = new Point2D(0, 0);
            looser = new Point2D(0, 0);
            double inputDim = (double)(input.Width * input.Height);
            
            double recurrentDim = 1.0;
            double recurrentContribution = 1.0;
            if (recurrentWeights != null)
                recurrentDim = output.Width * output.Height;

            ForEach(activity, false, (x, y) => 
            {
                activity[x, y] /= inputDim;

                if (recurrentWeights != null)
                {
                    recurrentActivity[x,y] /= recurrentDim;
                    activity[x, y] = (activity[x, y] + recurrentContribution * recurrentActivity[x, y]) / (1.0 + recurrentContribution);
                }

                if (activity[x,y]>activity[(int)winner.X, (int)winner.Y])
                {
                    winner.X = x;
                    winner.Y = y;
                }

                if (activity[x, y] < activity[(int)looser.X, (int)looser.Y])
                {
                    looser.X = x;
                    looser.Y = y;
                }
            });

            //Scale activity in [0,1]
            looserActivity = activity[(int)looser.X, (int)looser.Y];
            winnerActivity = activity[(int)winner.X, (int)winner.Y];
            double range = winnerActivity - looserActivity;

            ForEach(activity, true, (x, y) =>
            {
                activity[x, y] = (activity[x, y] - looserActivity) / range;
            });

            //Propagate to output
            if (useWinnerPositionAsOutput)
            {
                output.prediction[0,0] = winner.X / (double)activity.GetLength(0);
                output.prediction[1, 0] = winner.Y / (double)activity.GetLength(1);
            }
            else
            {
                Array.Copy(activity, output.prediction, activity.Length);
            }
        }

        protected override void topDown()
        {   
            //Make the prediction
            if (useWinnerPositionAsOutput)
            {
                //Set the winner to 1.0, all the rest to 0.0 (not really required)
                winner = new Point2D((int)(output.reality[0, 0] * activity.GetLength(0)), (int)(output.reality[1, 0] * activity.GetLength(1)));
                ForEach(activity, true, (x, y) =>
                {
                    activity[x, y] = (x == winner.X && y == winner.Y) ?  1.0 : 0.0;
                });

                //Directly take the winner RF as the prediction
                ForEach(input.reality, true, (x, y) =>
                {
                    input.prediction[x, y] = weights[x, y, (int)winner.X, (int)winner.Y];
                });
            }
            else
            {
                //Copy the output real to activity
                Array.Copy(output.reality, activity, activity.Length);

                //Scale in [0,1] && find winner
                winner = new Point2D(0, 0);
                looser = new Point2D(0, 0);

                ForEach(activity, false, (x, y) =>
                {
                    if (activity[x, y] > activity[(int)winner.X, (int)winner.Y])
                    {
                        winner.X = x;
                        winner.Y = y;
                    }

                    if (activity[x, y] < activity[(int)looser.X, (int)looser.Y])
                    {
                        looser.X = x;
                        looser.Y = y;
                    }
                });

                //Scale activity in [0,1]
                looserActivity = activity[(int)looser.X, (int)looser.Y];
                winnerActivity = activity[(int)winner.X, (int)winner.Y];
                double range = winnerActivity - looserActivity;

                ForEach(activity, true, (x, y) =>
                {
                    activity[x, y] = (activity[x, y] - looserActivity) / range;
                });

                //Zero the inputs
                double[,] contribution = new double[input.Width, input.Height];
                ForEach(input.reality,true, (x, y) =>
                {
                    input.prediction[x, y] = 0.0;
                    contribution[x, y] = 0.0;
                });

                //Take the mean of the weights, weighted by the activity of the target
                //Could be replaced by SoftMax
                ForEach(weights,false, (x1, y1, x2, y2) =>
                {
                    if (activity[x2, y2] > 0.95)
                    {
                        input.prediction[x1, y1] += weights[x1, y1, x2, y2] * activity[x2, y2];
                        contribution[x1, y1] += activity[x2, y2];
                    }
                });

                ForEach(input.reality,true, (x, y) =>
                {
                    input.prediction[x, y] /= contribution[x, y];
                });
            }
        }

        /// <summary>
        /// Combine adapted clones of this node by fusing their modifications.
        /// If possible.
        /// </summary>
        /// <param name="bag">An enumerable containing all the adapted nodes</param>
        public override void fuse(IEnumerable<IONodeAdaptive> bag)
        {
            double[, , ,] deltaWeights = new double[weights.GetLength(0), weights.GetLength(1), weights.GetLength(2), weights.GetLength(3)];

            foreach(IONodeAdaptive n in bag)
            {
                if (n is IONodeAdaptiveSOM)
                {
                    IONodeAdaptiveSOM clone = n as IONodeAdaptiveSOM;

                    for (int i = 0; i < 4; i++)
			        {
			            if (weights.GetLength(i) != clone.weights.GetLength(i))
                        {
                            throw new Exception("Trying to fuse IONodeAdaptiveSOM of different dimensions.");
                        }
			        }

                    ForEach(weights,false, (x1, y1, x2, y2) =>
                        {
                            deltaWeights[x1, y1, x2, y2] += (clone.weights[x1, y1, x2, y2] - weights[x1, y1, x2, y2]);
                        }
                        );
                }
                else
                {
                    throw new Exception("Trying to fuse IONodeAdaptive of different subtypes.");
                }
            }

            //Add the mean of the deltas
            ForEach(weights,true, (x1, y1, x2, y2) =>
            {
                weights[x1, y1, x2, y2] += deltaWeights[x1, y1, x2, y2] / (double)bag.Count();
            }
            );
        }

        #region Foreach helpers
        /// <summary>
        /// Use delegate instead of multiplying the foor loops
        /// </summary>
        /// <param name="operation"></param>
        private void ForEach(double[,,,] array, bool isParallel, Operation4D operation)
        {
            if (isParallel)
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    Parallel.For(0, array.GetLength(1), j =>
                    {
                        Parallel.For(0, array.GetLength(2), mi =>
                        {
                            Parallel.For(0, array.GetLength(3), mj =>
                            {
                                operation(i, j, mi, mj);
                            });
                        });
                    });
                });
            }
            else
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        for (int mi = 0; mi < array.GetLength(2); mi++)
                        {
                            for (int mj = 0; mj < array.GetLength(3); mj++)
                            {
                                operation(i, j, mi, mj);
                            }
                        }
                    }
                }
            }
        }

        private void ForEach(double[,] array, bool isParallel, Operation2D operation)
        {
            if (isParallel)
            {
                Parallel.For(0, array.GetLength(0), mi =>
                {
                    Parallel.For(0, array.GetLength(1), mj =>
                    {
                        operation(mi, mj);
                    });
                });
            }
            else
            {
                for (int mi = 0; mi < array.GetLength(0); mi++)
                {
                    for (int mj = 0; mj < array.GetLength(1); mj++)
                    {
                        operation(mi, mj);
                    }
                }
            }
        }

        //private void ForEachActivity(DoubleOperation operation)
        //{
        //    for (int mi = 0; mi < activity.GetLength(0); mi++)
        //    {
        //        for (int mj = 0; mj < activity.GetLength(1); mj++)
        //        {
        //            operation(activity[mi, mj]);
        //        }
        //    }
        //}

        //private void ForEachWeight(DoubleOperation operation)
        //{
        //    for (int i = 0; i < input.Width; i++)
        //    {
        //        for (int j = 0; j < input.Height; j++)
        //        {
        //            for (int mi = 0; mi < activity.GetLength(0); mi++)
        //            {
        //                for (int mj = 0; mj < activity.GetLength(1); mj++)
        //                {
        //                    operation(weights[i, j, mi, mj]);
        //                }
        //            }
        //        }
        //    }
        //}
    }
        #endregion
}
