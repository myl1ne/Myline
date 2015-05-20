using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace KaggleTaxis
{
    class TaxiCourse
    {
        public string tripID;
        public string callType;
        public string originCall;
        public string originStand;
        public int taxiID;
        public int timeStamp;
        public string dayType;
        public bool missingData;
        public List<double[]> polyline;

        public TaxiCourse()
        { }

        public TaxiCourse(string[] fields, double skipStepSize)
        {
            tripID = fields[0];
            callType = fields[1];
            originCall = fields[2];
            originStand = fields[3];
            taxiID = Convert.ToInt32(fields[4]);
            timeStamp = Convert.ToInt32(fields[5]);
            dayType = fields[6];
            missingData = Convert.ToBoolean(fields[7]);

            polyline = new List<double[]>();
            string[] delimiters = new String[]
            {
                ",",    // ,
                "\"",      // "
                "[[",     
                "[",      
                "]",      
                "]]",     
            };
            string[] points = fields[8].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (int p = 0; p < points.Length - 1; p += 2)
            {
                double[] pt = new double[]{ Convert.ToDouble(points[p]), Convert.ToDouble(points[p + 1]) };
                if (polyline.Count>0 && CDZNET.MathHelpers.distance(polyline.Last(), pt) > skipStepSize)
                    polyline.Add(pt);
            }
        }
    }
}
