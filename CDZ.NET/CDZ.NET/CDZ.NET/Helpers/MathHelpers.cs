using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET
{

    public enum Connectivity { square, torus };

    public static class MathHelpers
    {
        public static Random Rand = new Random();

        public static float distance(Point2D a, Point2D b, Connectivity connectivity, float width = 1.0f, float height = 1.0f)
        {
            return distance(a.X, a.Y, b.X, b.Y, connectivity, width, height);
        }

        public static double distance(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                throw new IndexOutOfRangeException("Incongruent vector sizes.");

            double euclideanDistance = 0.0f;
            for (int i = 0; i < a.Length; i++)
            {
                euclideanDistance += Math.Pow(b[i] - a[i], 2.0);
            }
            euclideanDistance = Math.Sqrt(euclideanDistance);

            return euclideanDistance;
        }

        public static double distance(double[] a, double[] b)
        {
            if (a.Length != b.Length)
                throw new IndexOutOfRangeException("Incongruent vector sizes.");

            double euclideanDistance = 0.0f;
            for (int i = 0; i < a.Length; i++)
            {
                euclideanDistance += Math.Pow(b[i] - a[i], 2.0);
            }
            euclideanDistance = Math.Sqrt(euclideanDistance);

            return euclideanDistance;
        }

        public static float distance(float x1, float y1, float x2, float y2, Connectivity connectivity, float width = 1.0f, float height = 1.0f)
        {
            double d = 0.0;
            double dX = Math.Abs(x1 - x2);
            double dY = Math.Abs(y1 - y2);

            double euclideanDistance = Math.Sqrt(Math.Pow(dX, 2.0) + Math.Pow(dY, 2.0));
            if (connectivity == Connectivity.square)
                d = euclideanDistance;
            else if (connectivity == Connectivity.torus)
            {
                double tdX = Math.Abs(x1 + (width - x2));
                double tdY = Math.Abs(y1 + (height - y2));
                d = Math.Sqrt(Math.Pow(Math.Min(dX, tdX), 2.0) + Math.Pow(Math.Min(dY, tdY), 2.0));
            }
            else
                Console.WriteLine("Error : No distance function defined for this connectivity pattern.");
            return (float)d;
        }
        public static float distanceManhatan(float x1, float y1, float x2, float y2, Connectivity connectivity, float width = 1.0f, float height = 1.0f)
        {
            double d = 0.0;
            double dX = Math.Abs(x1 - x2);
            double dY = Math.Abs(y1 - y2);

            double mDistance = dX+dY;
            if (connectivity == Connectivity.square)
                d = mDistance;
            else if (connectivity == Connectivity.torus)
            {
                double tdX = Math.Abs(x1 + (width - x2));
                double tdY = Math.Abs(y1 + (height - y2));
                d = Math.Min(dX, tdX)+ Math.Min(dY, tdY);
            }
            else
                Console.WriteLine("Error : No distance function defined for this connectivity pattern.");
            return (float)d;
        }
        public static float Sigmoid(float x, float lambda = 10.0f)
        {
            return 1.0f / (float)(1.0f + Math.Exp(-lambda * x));
        }

        public static void Clamp(ref float value, float min, float max)
        {
            value = (float)Math.Max(min, Math.Min(max, value));
        }
    }
}
