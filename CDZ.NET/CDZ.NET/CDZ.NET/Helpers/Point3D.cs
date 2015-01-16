using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET
{
    public class Point3D: Point2D
    {
        public float Z = 0.0f;

        public Point3D():base()
        { }

        public Point3D(float x, float y, float z):base(x,y)
        {
            Z = z;
        }

        public override bool Equals (Object obj)
        {
            Point3D p = obj as Point3D;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && Z == p.Z;
        }

        public bool Equals(Point3D p)
        {
            // Return true if the fields match:
            return base.Equals((Point2D)p) && Z == p.Z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Z.GetHashCode();
        }

//Overload of == because we always want to compare by value for this type
        public static bool operator ==(Point3D a, Point3D b)
        {
            if ((Point2D)a != (Point2D)b)
                return false;

            // Return true if the fields match:
            return a.Z == b.Z;
        }

        public static bool operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }
    }
}
