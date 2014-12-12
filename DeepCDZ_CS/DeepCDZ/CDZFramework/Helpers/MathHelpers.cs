using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZFramework
{
    public struct Coordinates3D
    {
        public float x; public float y; public float z;
        public Coordinates3D(float x, float y, float z) { this.x = x; this.y = y; this.z = z;}
    }
    public struct Coordinates2D
    {
        public float x; public float y;
        public Coordinates2D(float x, float y) { this.x = x; this.y = y; }
    }
    public enum Connectivity { square, torus };

    public static class MathHelpers
    {
        public static Random Rand = new Random();

        public static float distance(Coordinates2D a, Coordinates2D b, Connectivity connectivity, float width = 1.0f, float height = 1.0f)
        {
            return distance(a.x, a.y, b.x, b.y, connectivity, width, height);
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

        public static float Sigmoid(float x, float lambda = 10.0f)
        {
            return 1.0f / (float)(1.0f + Math.Exp(-lambda * x));
        }

        public static void Clamp(ref float value, float min, float max)
        {
            value = (float)Math.Max(0.0, Math.Min(1.0, value));
        }
    }
}
