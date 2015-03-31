using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDZNET.Helpers;

namespace CDZNET.Core
{
    [Serializable]
    public class Signal
    {
        public double[,] reality;
        public double[,] prediction;
        public int Width { get { return reality.GetLength(0); } }
        public int Height { get { return reality.GetLength(1); } }
        public Point2D Size { get { return new Point2D(Width, Height); } }

        public Signal(int w, int h)
        {
            reality = new double[w, h];
            prediction = new double[w, h];
        }
        public Signal(Signal s)
        {
            reality = new double[s.Width, s.Height];
            prediction = new double[s.Width, s.Height];
        }

        /// <summary>
        /// Compute the difference between reality and prediction using a parralel loop
        /// </summary>
        /// <returns>The error signal</returns>
        public double[,] ComputeError()
        {
            double[,] error = reality.Clone() as double[,];
            ArrayHelper.ForEach(reality, true, (x, y) => { error[x, y] -= prediction[x, y];});
            return error;
        }

        /// <summary>
        /// Compute the mean squarred error
        /// </summary>
        /// <returns>The error signal</returns>
        public double ComputeMeanSquarredError()
        {
            double error = 0.0;
            ArrayHelper.ForEach(reality, false, (x, y) => { error += Math.Pow(reality[x,y] - prediction[x, y], 2.0); });
            return Math.Sqrt(error) / (Width * Height);
        }

        /// <summary>
        /// Compute the mean absolute error
        /// </summary>
        /// <returns>The error signal</returns>
        public double ComputeMeanAbsoluteError()
        {
            double error = 0.0;
            ArrayHelper.ForEach(reality, false, (x, y) => { error += Math.Abs(reality[x, y] - prediction[x, y]); });
            return error / (Width * Height);
        }

        /// <summary>
        /// Compute the max absolute error
        /// </summary>
        /// <returns>The error signal</returns>
        public double ComputeMaxAbsoluteError()
        {
            double error = 0.0;
            ArrayHelper.ForEach(reality, false, (x, y) => { double tmpError = Math.Abs(reality[x, y] - prediction[x, y]); if (tmpError > error) error = tmpError; });
            return error;
        }

        /// <summary>
        /// Compute the sum of absolute error
        /// </summary>
        /// <returns>The error signal</returns>
        public double ComputeSumAbsoluteError()
        {
            double error = 0.0;
            ArrayHelper.ForEach(reality, false, (x, y) => { error += Math.Abs(reality[x, y] - prediction[x, y]);});
            return error;
        }
    }
}
