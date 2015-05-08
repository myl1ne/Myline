using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDZNET.Core;
using CDZNET;

namespace VowelWorldModel
{
    static class MMNodeFactory
    {

        public enum Model { DeepBelief, AFSOM, MLP, LUT, SOM, MWSOM, Matlab };
        public static MMNode obtain(Model mdl, int nbNeurons = 20)
        {
            MMNode node = null;
            switch (mdl)
            {
                case Model.SOM:
                    node = new CDZNET.Core.MMNodeSOM(new CDZNET.Point2D(nbNeurons, nbNeurons), false); //Here you specify which algo to be used
                    (node as MMNodeSOM).learningRate = 0.1;
                    (node as MMNodeSOM).elasticity = 2.0;
                    (node as MMNodeSOM).activityRatioToConsider = 1.0;
                    break;

                case Model.LUT:
                    node = new CDZNET.Core.MMNodeLookupTable(new Point2D(1, 1)); //Here you specify which algo to be used
                    (node as MMNodeLookupTable).TRESHOLD_SIMILARITY = 0.01;
                    (node as MMNodeLookupTable).learningRate = 0.5;
                    break;

                //case Model.Matlab:
                //    node = new CDZNET.Core.MMNodeMatLab(new CDZNET.Point2D(1, 1),            //This is the size of the output (so far not set in matlab case)
                //        "CA3",                                                              //This is the name of the variable corresponding to this node in Matlab
                //        "D:/robotology/src/Myline/CDZ.NET/CDZ.NET/CDZ.NET/Core/MM/Matlab",  //Path where the script is located
                //        "dummyConvergenceDivergence"                                        //name of the function/script
                //        );
                //    break;

                case Model.MWSOM:
                    node = new CDZNET.Core.MMNodeMWSOM(new CDZNET.Point2D(nbNeurons, nbNeurons));
                    break;

                case Model.DeepBelief:
                    node = new CDZNET.Core.MMNodeDeepBeliefNetwork(new CDZNET.Point2D(1, 1), new int[] { nbNeurons * nbNeurons });
                    break;

                case Model.AFSOM:
                    node = new CDZNET.Core.MMNodeAFSOM(new CDZNET.Point2D(nbNeurons, nbNeurons));
                    break;

                case Model.MLP:
                    node = new CDZNET.Core.MMNodeMLP(new CDZNET.Point2D(1, 1), 75, 25, 25, 75); //Here you specify which algo to be used
                    break;

                default:
                    throw new Exception("Unknown model type.");
            }
            return node;
        }
    }
}
