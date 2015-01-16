using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// An IO node that adaptes itself after each BottomUp or TopDown call.
    /// Uses the SOM algorithm. 
    /// The dimension of the output is either 2 (position of the winner in [0,1] or the dimension of the 
    /// map itself.
    /// </summary>
    public class IONodeAdaptiveSOM:IONodeAdaptive
    {
        //Parameters
        public double learningRadius = 3.0;
        public double learningRate = 0.1;

        private delegate void Operation2D(int x, int y);
        private delegate void Operation4D(int x1, int y1, int x2, int y2);
        private delegate void DoubleOperation(double a);

        double[,] activity;
        double[, , ,] weights;
        Point2D winner;
        Point2D looser;
        bool useWinnerPositionAsOutput;

        /// <summary>
        /// Constructs a new SOM node.
        /// </summary>
        /// <param name="inputDim">The dimension of the input</param>
        /// <param name="mapSize">The dimension of the map</param>
        /// <param name="useOnlyWinnerAsOutput">Should only the position of the winner be used as an output. False means the whole map activity will be used.</param>
        public IONodeAdaptiveSOM(Point2D inputDim, Point2D mapSize, bool useOnlyWinnerAsOutput = true)
            :base(inputDim, (useOnlyWinnerAsOutput)?new Point2D(2,1):mapSize)
        {
            activity = new double[(int)mapSize.X, (int)mapSize.Y];
            weights = new double[(int)inputDim.X, (int)inputDim.Y, (int)mapSize.X, (int)mapSize.Y];
            useWinnerPositionAsOutput = useOnlyWinnerAsOutput;
            winner = new Point2D(0,0);

            //Assign the weights randomly
            ForEach(weights, (x1,y1,x2,y2) => { weights[x1,y1,x2,y2] = MathHelpers.Rand.NextDouble(); });
        }

        public override void bottomUpAdaptation(object sender, EventArgs argsNull)
        {
            double squaredRadius2 = 2 * learningRadius * learningRadius;

            ForEach(weights, (x1, y1, x2, y2) =>
            {
                double distanceToWinner = MathHelpers.distance(x2, y2, winner.X, winner.Y, Connectivity.torus, activity.GetLength(0), activity.GetLength(1));
                double factor = Math.Exp(-(double)(distanceToWinner) / squaredRadius2);
                weights[x1, y1, x2, y2] += learningRate * factor * (input.x[x1, y1] - weights[x1, y1, x2, y2]);
            });
        }

        public override void topDownAdaptation(object sender, EventArgs argsNull)
        {
            //Here could we self organize as the other direction ?
        }

        protected override void bottomUp()
        {
            //Zero the activity
            ForEach(activity, (i,j) => { activity[i,j] = 0.0; });

            //Compute the activity
            ForEach(weights, (x1, y1, x2, y2) =>
                {
                    activity[x2, y2] += 1.0 - Math.Abs(weights[x1, y1, x2, y2] - input.x[x1, y1]);
                }
                );

            //Scale in [0,1] && find winner
            winner = new Point2D(0, 0);
            looser = new Point2D(0, 0);
            double inputDim = (double)(input.Width * input.Height);
            ForEach(activity, (x, y) => 
            { 
                activity[x,y] /= inputDim; 
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
            double min = activity[(int)looser.X, (int)looser.Y];
            double max = activity[(int)winner.X, (int)winner.Y];
            double range = max - min;

            ForEach(activity, (x, y) =>
            {
                activity[x, y] = (activity[x, y] - min) / range;
            });

            //Propagate to output
            if (useWinnerPositionAsOutput)
            {
                output.x[0,0] = winner.X / (double)activity.GetLength(0);
                output.x[1,0] = winner.Y / (double)activity.GetLength(1);
            }
            else
            {
                Array.Copy(activity, output.x, activity.Length);
            }
        }

        protected override void topDown()
        {
            if (useWinnerPositionAsOutput)
            {
                //Set the winner to 1.0, all the rest to 0.0 (not really required)
                winner = new Point2D((int)(output.x[0, 0] * activity.GetLength(0)), (int)(output.x[1, 0] * activity.GetLength(1)));
                ForEach(activity, (x, y) =>
                {
                    activity[x, y] = (x == winner.X && y == winner.Y) ?  1.0 : 0.0;
                });

                //Directly take the winner RF as the prediction
                ForEach(input.x, (x, y) =>
                {
                    input.x[x, y] = weights[x,y,(int)winner.X,(int)winner.Y];
                });
            }
            else
            {
                //Copy the output to activity (not really required)
                Array.Copy(output.x, activity, activity.Length);

                //Zero the inputs
                double[,] contribution = new double[input.Width, input.Height];
                ForEach(input.x, (x, y) =>
                {
                    input.x[x, y] = 0.0;
                    contribution[x, y] = 0.0;
                });

                //Take the mean of the weights, weighted by the activity of the target
                //Could be replaced by SoftMax
                ForEach(weights, (x1, y1, x2, y2) =>
                {
                    if (activity[x2, y2] > 0.95)
                    {
                        input.x[x1, y1] += weights[x1, y1, x2, y2] * activity[x2, y2];
                        contribution[x1, y1] += activity[x2, y2];
                    }
                });

                ForEach(input.x, (x, y) =>
                {
                    input.x[x, y] /= contribution[x, y];
                });
            }
        }

        #region Foreach helpers
        /// <summary>
        /// Use delegate instead of multiplying the foor loops
        /// </summary>
        /// <param name="operation"></param>
        private void ForEach(double[,,,] array, Operation4D operation)
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

        private void ForEach(double[,] array, Operation2D operation)
        {
            for (int mi = 0; mi < array.GetLength(0); mi++)
            {
                for (int mj = 0; mj < array.GetLength(1); mj++)
                {
                    operation(mi, mj);
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
