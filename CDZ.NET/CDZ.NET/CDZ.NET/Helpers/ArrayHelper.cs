using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Helpers
{
    public static class ArrayHelper
    {
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
