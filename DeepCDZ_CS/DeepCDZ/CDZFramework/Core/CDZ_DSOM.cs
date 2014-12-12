using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using CDZFramework;

namespace CDZFramework.Core
{
    public class CDZ_DSOM:CDZ
    {
        #region Parameters
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float Elasticity { get; set; }
        public float LearningRate { get; set; }
        #endregion

        #region Fields
        Neuron[,] neurons;
        Neuron winner = null;
        Neuron looser = null;
        Dictionary<Neuron, Coordinates2D> coordinates;
        public Neuron Neuron(int x, int y) { return neurons[x, y]; }
        #endregion

        public CDZ_DSOM(int width, int height, float learningRate = 0.01f, float elasticity = 2.0f)
            : base()
        {
            Width = width;
            Height = height;
            Elasticity = elasticity;
            LearningRate = learningRate;
            neurons = new Neuron[Width, Height];
            coordinates = new Dictionary<Neuron, Coordinates2D>(Width * height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    neurons[x, y] = new Neuron();
                    coordinates[neurons[x, y]] = new Coordinates2D(x, y);
                    registerExpert(neurons[x, y]);
                }
            }
        }
        
        //---------------------------------------------------------//
        /// <summary>
        /// Should be called once all the calls to AddModality have been done.
        /// </summary>
        public override void configure()
        {
            foreach (Neuron n in neurons)
            {
                foreach (Modality m in modalities)
                {
                    n.weights[m] = new float[m.Size];
                    for (int i = 0; i < m.Size; i++)
                    {
                        n.weights[m][i] = (float)MathHelpers.Rand.NextDouble();
                    }
                }
            }
        }

        //---------------------------------------------------------//
        #region Convergence
        protected override void preConvergence()
        {
            foreach (Neuron n in neurons)
                n.activity = 0.0f;
        }

        protected override void convergenceFrom(Modality mod)
        {
            //Parallel.For(0,Width,x=>
            for(int x=0;x<Width;x++)
            {
                //Parallel.For(0, Height, y =>
                for(int y=0;y<Width;y++)
                {
                    Neuron n = neurons[x, y];
                    float contribution = 0.0f;
                    for (int i = 0; i < mod.Size; i++)
                    {
                        contribution += (float)Math.Pow(mod.RealValues[i] - n.weights[mod][i], 2.0);
                    }
                    contribution = (float) ( Math.Sqrt(contribution) / (float)mod.Size );
                    n.activity += modalitiesInfluences[mod] * contribution;
                }//);
            }//);
        }
        protected override void postConvergence()
        {
            //Divide by the influence sum and keep track of winner
            float influenceSum = modalitiesInfluences.Values.Sum();
            winner = null;
            looser = null;
            if (influenceSum == 0.0f)
                influenceSum = 1.0f;

            foreach (Neuron n in neurons)
            {
                n.activity /= influenceSum;
                n.activity = 1 - n.activity;
                if (winner == null || n.activity > winner.activity)
                    winner = n;
                if (looser == null || n.activity < looser.activity)
                    looser = n;
            }
        }

        public override float GetConfidence()
        {
            if (winner != null)
                return winner.activity;
            else
                return 0.0f;
        }

        #endregion

        //---------------------------------------------------------//
        #region Divergence
        protected override void preDivergence()
        {

        }

        protected override void divergenceTo(Modality mod)
        {
            for (int i = 0; i < mod.Size; i++)
            {
                mod.PredictedValues[i] = 0.0f;
            }

            float totalContribution = 0.0f;
            foreach (Neuron n in neurons)
            {
                if (considerForPrediction(n))
                {
                    for (int i = 0; i < mod.Size; i++)
                    {
                        mod.PredictedValues[i] += n.activity * n.weights[mod][i];
                    }
                    totalContribution += n.activity;
                }
            }

            for (int i = 0; i < mod.Size; i++)
            {
                mod.PredictedValues[i] /= totalContribution;
            }
        }
        protected override void postDivergence()
        {

        }
        bool considerForPrediction(Neuron n)
        {
            return n == winner;
            //return (n.activity >= 0.95*winner.activity);
        }

        #endregion

        //---------------------------------------------------------//
        #region Training
        //---------------------------------------------------------//

        public override void Train()
        {
            float elasticityFactor = (float)(-1.0f / Math.Pow(Elasticity, 2.0));
            float winnerSquaredError = (float)Math.Pow(1.0-winner.activity, 2.0);
            float winX = coordinates[winner].x;
            float winY = coordinates[winner].y;
            //Parallel.For(0,Width,x=>
            for(int x=0;x<Width;x++)
            {
                //Parallel.For(0, Height, y =>
                for(int y=0;y<Height;y++)
                {
                    Neuron n = neurons[x, y];
                    float distanceSquared = (float)Math.Pow( MathHelpers.distance(x,y,winX,winY, Connectivity.torus)/Math.Sqrt(Width*Height), 2.0);
                    float heta = (float)Math.Exp(elasticityFactor * distanceSquared / (float.Epsilon+winnerSquaredError));

                    foreach (Modality m in n.weights.Keys)
                    {
                        for (int i = 0; i < n.weights[m].Count(); i++)
                        {
                            float error;
                            float dW;
                            if (m.tag == "TopDown")
                            {
                                error = winner.weights[m][i] - n.weights[m][i];
                            }
                            else
                            {
                                error = m.RealValues[i] - n.weights[m][i];
                            }
                            dW = LearningRate * modalitiesLearning[m] * (1 - n.activity) * heta * error;
                            n.weights[m][i] += dW;
                            MathHelpers.Clamp(ref n.weights[m][i], 0.0f, 1.0f);
                        }
                    }
                }//);
            }//);
        }
        #endregion

        //---------------------------------------------------------//
        #region Receptive Fields Plotting
        public override int computeBestExpert(Modality m, float[] value)
        {
            Neuron best = null;
            float bestDistance = 1.0f;
            foreach (Neuron n in neurons)
            {
                float distance = 0.0f;
                for (int i = 0; i < m.Size; i++)
                {
                    distance += 1.0f - (float)Math.Pow(value[i] - n.weights[m][i], 2.0);
                }
                distance /= m.Size;

                if (distance <= bestDistance)
                {
                    best = n;
                    bestDistance = distance;
                }
            }
            return getExpertIndex(best);
        }
        

        public override Dictionary<Modality, float[]> getReceptiveField(int expertIndex)
        {
            return (getExpertAtIndex(expertIndex) as Neuron).weights;
        }
        #endregion

        #region Activity Plotting
        /// <summary>
        /// Returns a bitmap of the current activity of this layer
        /// </summary>
        /// <param name="a">Color for 0.0 activity</param>
        /// <param name="b">Color for 1.0 activity</param>
        /// <param name="useNormalisation">Should the activity be normalized</param>
        /// <returns></returns>
        public Bitmap asBitmap(bool useNormalisation = false)
        {
            double min = 0.0;
            double max = 1.0;
            if (useNormalisation)
            {
                min = looser.activity;
                max = winner.activity;
            }

            double range = max - min;

            Bitmap bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb); ;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = ColorInterpolator.InterpolateBetween(Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 0), Color.FromArgb(255, 0, 0), (neurons[x, y].activity - min) / (range));
                    bmp.SetPixel(x, y, color);
                }
            }
            return bmp;
        }

        /// <summary>
        /// Set the activity of a layer given a bitmap. If the bitmap is colored then it will be converted to grayscale.
        /// </summary>
        /// <param name="a">Color for 0.0 activity</param>
        /// <param name="b">Color for 1.0 activity</param>
        /// <param name="useNormalisation">Should the activity be normalized</param>
        /// <returns></returns>
        public void fromBitmap(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color px = bmp.GetPixel(x, y);
                    float value = 0.2126f * px.R + 0.7152f * px.G + 0.0722f * px.B;
                    neurons[x, y].activity = value / 255.0f;
                }
            }
        }
        #endregion
    }
}
