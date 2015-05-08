using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HippocampusSimple
{
    class TaxiCourse
    {
        string tripID;
        string callType;
        int originCall;
        int originStand;
        int taxiID;
        int timeStamp;
        string dayType;
        bool missingData;
        List<double[]> polyline;
    }
}
