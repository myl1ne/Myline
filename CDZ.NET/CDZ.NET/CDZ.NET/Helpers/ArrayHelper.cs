using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Helpers
{
    public static class ArrayHelper
    {
        public static double[] linearize(double[,] array)
        {
            double[] result = new double[array.Length];
            for (int mi = 0; mi < array.GetLength(0); mi++)
            {
                for (int mj = 0; mj < array.GetLength(1); mj++)
                {
                    result[mi * array.GetLength(1) + mj] = array[mi, mj];
                }
            }
            return result;
        }
        public static bool unlinearize(double[] line, double[,] array)
        {
            if (line.Length != array.GetLength(0) * array.GetLength(1))
                throw new Exception("The size of the line and the array do not match");

            for (int mi = 0; mi < array.GetLength(0); mi++)
            {
                for (int mj = 0; mj < array.GetLength(1); mj++)
                {
                    array[mi, mj] = line[mi * array.GetLength(1) + mj] ;
                }
            }
            return true;
        }

        public static void ForEach(double[,] array, bool isParallel, Action<int, int> operation)
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

        public static void ForEach(object[,] array, bool isParallel, Action<int, int> operation)
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
    }
}
