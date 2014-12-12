using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using CDZFramework.Core;

namespace CDZFramework.Networks
{
    public class DeepNetwork
    {
        public List<CDZ[,]> layers;
        Dictionary<CDZ, Coordinates3D> coordinates;

        int inputModalitySize;
        Dictionary<Modality, Modality> modalitiesLinks = new Dictionary<Modality, Modality>();

        public DeepNetwork(List<int> structure, float projectionRadius = 1.0f, int interlayersModalitiesSize = 3, int inputModalitySize = 0, float lateralConnectivityRadius=0.0f)
        {
            this.inputModalitySize = inputModalitySize;

            //CDZ
            layers = new List<CDZ[,]>();
            coordinates = new Dictionary<CDZ, Coordinates3D>();
            for (int i = 0; i < structure.Count; i++)
            {
                int size = structure[i];
                layers.Add(new CDZ[size, size]);

                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        //layers[i][x, y] = new CDZ_ESOM();
                        layers[i][x, y] = new CDZ_DSOM(10,10,0.01f);
                        coordinates.Add(layers[i][x, y], new Coordinates3D(x,y,i));
                    }
                }
            }

            //Input modalities
            if (inputModalitySize>0)
            {
                for (int x = 0; x < layers[0].GetLength(0); x++)
                {
                    for (int y = 0; y < layers[0].GetLength(1); y++)
                    {
                        layers[0][x, y].AddModality(inputModalitySize).tag = "BottomUp";
                    }
                }
            }

            //Connections Bottomup/TopDown
            for (int i = 1; i < structure.Count; i++)
            {
                for (int x1 = 0; x1 < layers[i-1].GetLength(0); x1++)
                {
                    for (int y1 = 0; y1 < layers[i-1].GetLength(1); y1++)
                    {
                        float fx1 = (1+x1) * (1.0f / (1.0f + layers[i - 1].GetLength(0)));
                        float fy1 = (1+y1) * (1.0f / (1.0f + layers[i - 1].GetLength(1)));
                        for (int x2 = 0; x2 < layers[i].GetLength(0); x2++)
                        {
                            for (int y2 = 0; y2 < layers[i].GetLength(1); y2++)
                            {
                                float fx2 = (1+x2) * (1.0f / (1.0f + layers[i].GetLength(0)));
                                float fy2 = (1+y2) * (1.0f / (1.0f + layers[i].GetLength(1)));

                                float distance = MathHelpers.distance(fx1, fy1, fx2, fy2, Connectivity.square) ;
                                if (distance <= projectionRadius)
                                {
                                    Modality src = layers[i - 1][x1, y1].AddModality(interlayersModalitiesSize,0.0001f,0.0001f); //We do not learn from the topdown, but get influenced by it
                                    Modality dest = layers[i][x2, y2].AddModality(interlayersModalitiesSize);
                                    modalitiesLinks.Add(src, dest);
                                    modalitiesLinks.Add(dest,src);
                                    src.tag = "TopDown";
                                    dest.tag = "BottomUp";
                                }
                            }
                        }
                    }
                }
            }

            //Connections Lateral
            for (int i = 0; i < structure.Count; i++)
            {
                List<KeyValuePair<CDZ,CDZ> > alreadyConnected = new List<KeyValuePair<CDZ,CDZ>>();
                for (int x1 = 0; x1 < layers[i].GetLength(0); x1++)
                {
                    for (int y1 = 0; y1 < layers[i].GetLength(1); y1++)
                    {
                        for (int x2 = 0; x2 < layers[i].GetLength(0); x2++)
                        {
                            for (int y2 = 0; y2 < layers[i].GetLength(1); y2++)
                            {
                                float distance = MathHelpers.distance(x1, y1, x2, y2, Connectivity.square);
                                if (distance <= lateralConnectivityRadius && distance != 0.0f)
                                {
                                    if (alreadyConnected.Contains(new KeyValuePair<CDZ, CDZ>(layers[i][x1, y1], layers[i][x2, y2]), new ADirectionalCDZPairComparer()))
                                    {
                                        //Skip
                                    }
                                    else
                                    {
                                        Modality src = layers[i][x1, y1].AddModality(interlayersModalitiesSize);
                                        Modality dest = layers[i][x2, y2].AddModality(interlayersModalitiesSize);
                                        modalitiesLinks.Add(src, dest);
                                        modalitiesLinks.Add(dest, src);
                                        src.tag = "Lateral";
                                        dest.tag = "Lateral";
                                        alreadyConnected.Add(new KeyValuePair<CDZ, CDZ>(layers[i][x1, y1], layers[i][x2, y2]));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Configure
            for (int i = 0; i < structure.Count; i++)
            {
                for (int x = 0; x < layers[i].GetLength(0); x++)
                {
                    for (int y = 0; y < layers[i].GetLength(1); y++)
                    {
                        layers[i][x, y].configure();
                    }
                }
            }
        }

        public class ADirectionalCDZPairComparer : IEqualityComparer< KeyValuePair<CDZ,CDZ> >
        {
            public bool Equals(KeyValuePair<CDZ, CDZ> x, KeyValuePair<CDZ, CDZ> y)
            {
                return (x.Key == y.Key && x.Value == y.Value ) || (x.Key==y.Value && x.Value==y.Key);
            }

            public int GetHashCode(KeyValuePair<CDZ,CDZ> obj)
            {
                return (obj.Key.GetHashCode() * obj.Value.GetHashCode());
            }
        }
        public void Update()
        {
            //Activate / Predict
            foreach(CDZ[,] l in layers)
            {
                //Parallel.For(0, l.GetLength(0), x => //Parallel.For(0, l.GetLength(0), x =>// for (int x = 0; x < l.GetLength(0); x++)
                for (int x = 0; x < l.GetLength(0); x++)
                {
                    //Parallel.For(0, l.GetLength(1), y =>
                    for (int y = 0; y < l.GetLength(1); y++)
                    {
                        l[x, y].Converge();
                        l[x, y].Diverge();
                    }//);
                }//);

                //Propage the information
                foreach (var m in modalitiesLinks)
                {
                    Modality src = m.Key;
                    Modality dest = m.Value;
                    dest.RealValues = src.PredictedValues; // potential bug, copy ?
                }
            }

            //Train
            foreach (CDZ[,] l in layers)
            {
                Parallel.For(0, l.GetLength(0), x =>//Parallel.For(0, l.GetLength(0), x =>// for (int x = 0; x < l.GetLength(0); x++)
                {
                    Parallel.For(0, l.GetLength(1), y =>//Parallel.For(0, l.GetLength(1), y =>//for (int y = 0; y < l.GetLength(1); y++)
                    {
                        l[x, y].Train();
                    });
                });
            }


        }

        public void setAsInput(Bitmap bmp)
        {
            int thumbW = bmp.Width / layers[0].GetLength(0);
            int thumbH = bmp.Height / layers[0].GetLength(1);

            for (int x = 0; x < layers[0].GetLength(0); x++)
            {
                for (int y = 0; y < layers[0].GetLength(1); y++)
                {
                    //The first modality is the one that was optionaly created in the constructor. Or not... :-/
                    Bitmap croppedImage = bmp.Clone(
                        new Rectangle(x*thumbW,y*thumbH,thumbW,thumbH),
                        bmp.PixelFormat);
                    Modality.fromBitmap(layers[0][x, y].modalities[0].RealValues, croppedImage);
                }
            }
        }
        public Bitmap getInputPresented()
        {
            int modalityBmpSize = (int)Math.Sqrt(layers[0][0, 0].modalities[0].Size);
            int fullBmpSize = (int)Math.Sqrt(layers[0][0, 0].modalities[0].Size) * layers[0].GetLength(0);
            Bitmap output = new Bitmap(fullBmpSize, fullBmpSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < layers[0].GetLength(0); x++)
            {
                for (int y = 0; y < layers[0].GetLength(1); y++)
                {
                    Graphics g = Graphics.FromImage(output);
                    g.DrawImage(Modality.toBitmap(layers[0][x, y].modalities[0].RealValues), x * modalityBmpSize, y * modalityBmpSize, modalityBmpSize, modalityBmpSize);
                    g.Dispose();
                }
            }
            return output;
        }
        public Bitmap getInputPrediction()
        {
            int modalityBmpSize = (int)Math.Sqrt(layers[0][0,0].modalities[0].Size);
            int fullBmpSize = (int)Math.Sqrt(layers[0][0,0].modalities[0].Size) * layers[0].GetLength(0);
            Bitmap output = new Bitmap(fullBmpSize, fullBmpSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < layers[0].GetLength(0); x++)
            {
                for (int y = 0; y < layers[0].GetLength(1); y++)
                {
                    Graphics g = Graphics.FromImage(output);
                    g.DrawImage(Modality.toBitmap(layers[0][x, y].modalities[0].PredictedValues), x * modalityBmpSize, y * modalityBmpSize, modalityBmpSize, modalityBmpSize);
                    g.Dispose();
                }
            }
            return output;
        }


        #region RF Plotting
        public void fillRFBmp(ref Bitmap bmp, CDZ cdz, int expert)
        {
            //Create one image for each expert
            int expertsCount = cdz.getExpertsNumber();

            //Get the rf of this expert
            Dictionary<Modality, float[]> rf = cdz.getReceptiveField(expert);

            //Pass all the modalities that receive from this one, execpt the last one, which is topdown
            foreach (Modality bottomMod in rf.Keys)
            {
                //Skip the last bottomup mod
                if (bottomMod.tag != "BottomUp")
                    continue;

                //We reached a leaf
                if (!modalitiesLinks.ContainsKey(bottomMod))
                {
                    //Fill the BMP
                    Coordinates3D coo = coordinates[bottomMod.parent];
                    int sideSize = (int)Math.Sqrt(bottomMod.Size);
                    Rectangle rect = new Rectangle((int)coo.x * sideSize, (int)coo.y * sideSize, sideSize, sideSize);
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(Modality.toBitmap(rf[bottomMod]), rect);
                    g.Dispose();
                }
                else
                {
                    //Recursive call
                    fillRFBmp(ref bmp, modalitiesLinks[bottomMod].parent, expert);
                }
            }

        }

        public Bitmap createInputBitmap()
        {
            int modalityBmpSize = (int)Math.Sqrt(layers[0][0, 0].modalities[0].Size);
            int fullBmpSize = (int)Math.Sqrt(layers[0][0, 0].modalities[0].Size) * layers[0].GetLength(0);
            Bitmap output = new Bitmap(fullBmpSize, fullBmpSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            return output;
        }

        #endregion
    }
}
