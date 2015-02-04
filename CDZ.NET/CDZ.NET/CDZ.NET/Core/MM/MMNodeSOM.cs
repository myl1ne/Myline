using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    /// <summary>
    /// A multimodal node based on a Self Organizing Map. (MMCM).
    /// </summary>
    public class MMNodeSOM:MMNode
    {        
        //Parameters
        public double learningRate;
        public double elasticity;
        bool useWinnerPositionAsOutput;

        #region Delegates (helpers)
        private delegate void Operation2D(int x, int y);
        private delegate void Operation4D(int x1, int y1, int x2, int y2);
        private delegate void DoubleOperation(double a);
        #endregion

        #region Components
        double[,] activity;
        Dictionary<Signal, double[,] > activities;
        Dictionary<Signal, double[, , ,] > weights;
        Point2D winner;
        double winnerActivity;
        Point2D looser;
        double looserActivity;
        //double[,] previousOutput;
        //double[,] recurrentActivity;
        //double[, , ,] recurrentWeights;
        #endregion

        #region Accessors
        public int Width { get { return activity.GetLength(0); } }
        public int Height { get { return activity.GetLength(1); } }
        #endregion


        public MMNodeSOM(Point2D mapSize, bool useOnlyWinnerAsOutput = true)
            : base((useOnlyWinnerAsOutput) ? new Point2D(2, 1) : mapSize)
        {
            activity = new double[(int)mapSize.X, (int)mapSize.Y];
            activities = new Dictionary<Signal, double[,]>();
            weights = new Dictionary<Signal, double[, , ,]>();
            winner = new Point2D(0,0);
            looser = new Point2D(0,0);
            
            useWinnerPositionAsOutput = useOnlyWinnerAsOutput;
            learningRate = 0.1;
            elasticity = 4.0;

            this.onConvergence += HandleConvergence;
            this.onDivergence += HandleDivergence;
        }

        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);
            weights[s] = new double[s.Width, s.Height, Width, Height];
            activities[s] = new double[Width,Height];
            //Assign the weights randomly
            ForEach(weights[s], false, (x1, y1, x2, y2) => { weights[s][x1, y1, x2, y2] = MathHelpers.Rand.NextDouble(); });
        }


        /// <summary>
        /// Implementation of the convergence operation.
        /// </summary>
        protected override void converge() 
        { 
            //Zero the combined activity
            ForEach(activity, true, (i, j) => { activity[i, j] = 0.0; });
            double influenceSum = 0.0;        
            
            //Compute the activities and combine them
            foreach(Signal s in modalities)
            {
                influenceSum += modalitiesInfluence[s];
                double signalDim = s.Width*s.Height;
                //Zero the modality specific activity
                ForEach(activities[s], true, (i, j) => { activities[s][i,j] = 0.0; });
                
                //Compute it & add it to the combined activity
                ForEach(weights[s], false, (x1, y1, x2, y2) =>
                    {
                        activities[s][x2, y2] += (1.0 - Math.Abs(weights[s][x1, y1, x2, y2] - s.reality[x1, y1]))/signalDim;
                        activity[x2,y2] += modalitiesInfluence[s] * activities[s][x2, y2];
                    }
                    );
            }
            
            //Divide by the sum of influences && find winner/looser
            winner = new Point2D(0, 0);
            looser = new Point2D(0, 0);            
            ForEach(activity, false, (x, y) => 
            {
                activity[x, y] /= influenceSum;

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

            //Scale in [0,1]
            looserActivity = activity[(int)looser.X, (int)looser.Y];
            winnerActivity = activity[(int)winner.X, (int)winner.Y];
            double range = winnerActivity - looserActivity;

            ForEach(activity, true, (x, y) =>
            {
                activity[x, y] = (activity[x, y] - looserActivity) / range;
            });

            //Copy the map activity to the output
            if (useWinnerPositionAsOutput)
            {
                output.prediction[0, 0] = winner.X / (double)activity.GetLength(0);
                output.prediction[1, 0] = winner.Y / (double)activity.GetLength(1);
            }
            else
            {
                Array.Copy(activity, output.prediction, activity.Length);
            }
        }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected override void diverge() 
        {             
            foreach(Signal s in modalities)
            {
                //Directly take the winner RF as the prediction
                if (useWinnerPositionAsOutput)
                {
                    ForEach(s.prediction, true, (x, y) =>
                    {
                        s.prediction[x, y] = weights[s][x, y, (int)winner.X, (int)winner.Y];
                    });
                }
                else
                {
                    //Zero the inputs & allocate the contribution table
                    double[,] contribution = new double[s.Width, s.Height];
                    ForEach(s.prediction,true, (x, y) =>
                    {
                        s.prediction[x, y] = 0.0;
                        contribution[x, y] = 0.0;
                    });

                    //Take the mean of the weights, weighted by the activity of the target
                    //Could be replaced by SoftMax
                    ForEach(weights[s],false, (x1, y1, x2, y2) =>
                    {
                        if (activity[x2, y2] > 0.95)
                        {
                            s.prediction[x1, y1] += weights[s][x1, y1, x2, y2] * activity[x2, y2];
                            contribution[x1, y1] += activity[x2, y2];
                        }
                    });

                    ForEach(s.reality,true, (x, y) =>
                    {
                        s.prediction[x, y] /= contribution[x, y];
                    });
                } 
            }
        }

        /// <summary>
        /// Triggered after a convergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleConvergence(object o, EventArgs nullargs)
        {
            //Here we apply the weight adaptation
            bool USE_DSOM = true;            
            //Standard SOM
            double learningRadius = (1.0 / 4.0) * (activity.GetLength(0) + activity.GetLength(1)) / 2.0;
            double squaredRadius2 = 2 * learningRadius * learningRadius;
            //DSOM
            double winnerError = 1.0 - winnerActivity;
            double inversedSquaredElasticity = -(1 / (elasticity * elasticity));
            foreach (Signal s in modalities)
            {
                ForEach(weights[s], true, (x1, y1, x2, y2) =>
                {
                    double distanceToWinner = MathHelpers.distance(x2, y2, winner.X, winner.Y, Connectivity.torus, activity.GetLength(0), activity.GetLength(1));
                    double factor = Math.Exp(-(double)(distanceToWinner) / squaredRadius2);

                    if (USE_DSOM)
                        factor = learningRate * Math.Exp(inversedSquaredElasticity * (distanceToWinner / winnerError));
                    else
                        factor = learningRate * Math.Exp(-(double)(distanceToWinner) / squaredRadius2);

                    weights[s][x1, y1, x2, y2] += factor * (s.reality[x1, y1] - weights[s][x1, y1, x2, y2]);
                });
            }
        }

        /// <summary>
        /// Triggered after a divergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleDivergence(object o, EventArgs nullargs)
        {

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
        #endregion
    }
}
