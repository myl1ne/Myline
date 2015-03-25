using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDZNET.Core;
using CDZNET;

namespace VowelWorldModel
{
    static class MNNodeFactory
    {
        public enum Model { SOM, LUT, Matlab, MWSOM, DeepBelief };
        public static MMNode obtain(Model mdl)
        {
            MMNode node = null;
            switch (mdl)
            {
                case Model.SOM:
                    node = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(20, 20), false); //Here you specify which algo to be used
                    (node as MMNodeSOM).learningRate = 0.03;
                    (node as MMNodeSOM).elasticity = 2.0;
                    (node as MMNodeSOM).activityRatioToConsider = 1.0;
                    break;

                case Model.LUT:
                    node = new CDZNET.Core.MMNodeLookupTable(new Point2D(1, 1)); //Here you specify which algo to be used
                    (node as MMNodeLookupTable).TRESHOLD_SIMILARITY = 0.1;
                    (node as MMNodeLookupTable).learningRate = 0.1;
                    break;

                case Model.Matlab:
                    node = new CDZNET.Core.MMNodeMatLab(new CDZNET.Point2D(1, 1),            //This is the size of the output (so far not set in matlab case)
                        "CA3",                                                              //This is the name of the variable corresponding to this node in Matlab
                        "D:/robotology/src/Myline/CDZ.NET/CDZ.NET/CDZ.NET/Core/MM/Matlab",  //Path where the script is located
                        "dummyConvergenceDivergence"                                        //name of the function/script
                        );
                    break;

                case Model.MWSOM:
                    node = new CDZNET.Core.MMNodeMWSOM(new CDZNET.Point2D(30, 20));
                    break;

                case Model.DeepBelief:
                    node = new CDZNET.Core.MMNodeDeepBeliefNetwork(new CDZNET.Point2D(30, 20), new int[]{10});
                    break;

                default:
                    throw new Exception("Unknown model type.");
            }
            return node;
        }
    }
}
