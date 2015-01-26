using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    public class IONodeGridCells:IONode
    {
        class GridCell
        {
            //double[,] rf;
            double sigma = 0.05;
            double spacing = 0.1;
            double rfShift = 0.0;
            public GridCell(Point2D inputDim, double sigma, double spacing)
            {
                this.sigma = sigma;
                this.spacing = spacing;
                //rf = new double[(int)inputDim.X, (int)inputDim.Y];
                rfShift = MathHelpers.Rand.NextDouble() *  spacing/2.0;
                //for (int x = 0; x < inputDim.X; x++)
                //{
                //    for (int y = 0; y < inputDim.Y; y++)
                //    {
                //        rf[x,y] = MathHelpers.Rand.NextDouble() * spacing / 2.0;
                //    }
                //}
            }
            public double computeActivity(double[,] input)
            {
                double activity = 0.0;
                double sigmaSqr2 = 2 * sigma * sigma;
                for(int x=0;x<input.GetLength(0);x++)
                {                
                    for(int y=0;y<input.GetLength(1);y++)
                    {
                        double modulo = Math.Abs(input[x, y] % spacing);
                        double mInput = Math.Min(modulo, spacing - modulo);
                        activity += Math.Pow(mInput - rfShift, 2.0) / sigmaSqr2;
                    }
                }
                activity = Math.Exp(-activity);
                return activity;
            }
        }

        GridCell[,] cells;

        public IONodeGridCells(Point2D inputDim, Point2D outputDim, double rfSize, double spacing)
            : base(inputDim, outputDim)
        {
            cells = new GridCell[(int)outputDim.X, (int)outputDim.Y];

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y] = new GridCell(inputDim,rfSize, spacing);
                }
            }
        }

        protected override void bottomUp()
        {
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    output.prediction[x,y] = cells[x, y].computeActivity(input.reality);
                }
            }
        }

        protected override void topDown()
        {
            throw new NotImplementedException();
        }

    }
}
