using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CDZNET.Core
{
    [Serializable]
    /// <summary>
    /// Convolve (in a general sense) a signal.
    /// A sliding windows is moved over the input, it is used to input a filter and generates an output.
    /// 
    /// In the case of a stepSize of 1 and a filter output size of 1 the class acts as standard convolution.
    /// In the case of a stepSize having the size of the input of the filter, then the class acts as a sampling.
    /// 
    /// In the general case, the output of this class is of size inputSize * filter.ouputSize / stepSize
    /// 
    /// </summary>
    public class IONodeConvolutionParallel:IONode
    {
        IONode filter;
        Point2D stepSize;

        public IONodeConvolutionParallel(Point2D inputDim, IONode filter, Point2D stepSize)
            : base(inputDim, new Point2D(filter.output.Width * inputDim.X /stepSize.X , filter.output.Height * inputDim.Y / stepSize.Y))
        {
            this.filter = filter;
            this.stepSize = stepSize;
            //if (this.filter.input.Width % 2 == 0||this.filter.input.Height % 2 == 0)
            //{
            //    throw new ArgumentException("Filter input cannot be even.");
            //}
        }

        /// <summary>
        /// Move a sliding window and apply the filter by using its BottomUp() method.
        /// </summary>
        protected override void bottomUp()
        {
            System.Collections.Concurrent.ConcurrentStack<IONode> filterClones = new ConcurrentStack<IONode>();
            int stepsWidth = (int)(input.Width / stepSize.X);
            int stepsHeight = (int)(input.Height/ stepSize.Y);

            int totalIterations = stepsWidth * stepsHeight;

            for (int i = 0; i < totalIterations; i++)
            {
                IONode clone = ObjectCopier.Clone(filter);
                filterClones.Push(clone);
            }

            System.Collections.Concurrent.ConcurrentStack<IONode> filterClonesUsed = new ConcurrentStack<IONode>();

            //We pass all components of the input signal
            Parallel.For(0, stepsWidth, stepX =>
            {
                int xI = stepX * (int)stepSize.X;
                Parallel.For(0, stepsHeight, stepY =>
                {
                    int yI = stepY * (int)stepSize.Y;

                    //Clone the filter
                    IONode clone;
                    while (!filterClones.TryPop(out clone)) ;

                    //We construct the filter input based on its dimensions
                    for (int xF = 0; xF < clone.input.Width; xF++)
                    {
                        for (int yF = 0; yF < clone.input.Height; yF++)
                        {
                            int x = xI - clone.input.Width / 2 + xF;
                            int y = yI - clone.input.Height / 2 + yF;
                            x = Math.Min(input.Width - 1, Math.Max(x, 0));
                            y = Math.Min(input.Height - 1, Math.Max(y, 0));
                            clone.input.reality[xF, yF] = input.reality[x, y];
                        }
                    }

                    //We apply the clone
                    clone.BottomUp();

                    //We copy the result of the clone to the output
                    for (int xF = 0; xF < clone.output.Width; xF++)
                    {
                        for (int yF = 0; yF < clone.output.Height; yF++)
                        {
                            output.prediction[(int)(xI / stepSize.X) * clone.output.Width + xF, (int)(yI / stepSize.Y) * clone.output.Height + yF] = clone.output.prediction[xF, yF];
                        }
                    }
                    filterClonesUsed.Push(clone);
                });
            });

            if (filter is IONodeAdaptive)
            {
                IEnumerable<IONodeAdaptive> casted = filterClonesUsed.Cast<IONodeAdaptive>();
                (filter as IONodeAdaptive).fuse(casted);
            }
        }

        /// <summary>
        /// Moves a sliding window, apply the filter's TopDown() method.
        /// Calculate the mean of all the applications.
        /// </summary>
        protected override void topDown()
        {
            //We zero the input & keep track of the contributions to it
            double[,] inputContributions = new double[input.Width, input.Height];
            for (int xI = 0; xI < input.Width; xI ++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    input.prediction[xI, yI] = 0.0;
                    inputContributions[xI, yI] = 0.0;
                }
            }

            //We pass all components of the input signal and sum the results of the filter passes
            for (int xI = 0; xI <= input.Width - stepSize.X; xI += (int)stepSize.X)
            {
                for (int yI = 0; yI <= input.Height - stepSize.Y; yI += (int)stepSize.Y)
                {
                    //We construct the filter output based on its dimensions
                    for (int xF = 0; xF < filter.output.Width; xF++)
                    {
                        for (int yF = 0; yF < filter.output.Height; yF++)
                        {
                            filter.output.reality[xF, yF] = output.reality[(int)(xI / stepSize.X) * filter.output.Width + xF, (int)(yI / stepSize.Y) * filter.output.Height + yF]; 
                        }
                    }

                    //We apply the filter
                    filter.TopDown();

                    //We sum the results of the filter to the input
                    for (int xF = 0; xF < filter.input.Width; xF++)
                    {
                        for (int yF = 0; yF < filter.input.Height; yF++)
                        {
                            int x = xI - filter.input.Width / 2 + xF;
                            int y = yI - filter.input.Height / 2 + yF;
                            x = Math.Min(input.Width - 1, Math.Max(x, 0));
                            y = Math.Min(input.Height - 1, Math.Max(y, 0));
                            input.prediction[x, y] += filter.input.prediction[xF, yF];
                            inputContributions[x, y] += 1;
                        }
                    }
                }
            }

            //We divide by the number of contributions
            for (int xI = 0; xI < input.Width; xI++)
            {
                for (int yI = 0; yI < input.Height; yI++)
                {
                    input.prediction[xI, yI] /= inputContributions[xI, yI];
                }
            }
        }

    }
}
