using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CDZNET.Core.Neurons;
using CDZNET.Helpers;

namespace CDZNET.Core
{
    /// <summary>
    /// A multimodal node template.
    /// </summary>
    public class MMNodeMWSOM : MMNode
    {
        #region Runtime Parameters
        
        /// <summary>
        /// Number of cycles of presentation of a stimulus
        /// </summary>
        public int tau = 5;

        /// <summary>
        /// Tuning factor afferent vs lateral
        /// </summary>
        private double alpha = 0.2;

        /// <summary>
        /// The fixed weight for self connections. Not subject to learning.
        /// </summary>
        private double beta = 0.5;

        /// <summary>
        /// Activation dropoff
        /// </summary>
        public double gamma = 0.37;
        private double initialGamma = 0.37;
        private double finalGamma = 0.0;
        private double gammaInfl = 0.2;
        private double gammaSigma = 0.16;

        /// <summary>
        /// Learning rate for afferent connections. If > 0.1 then the learning happens in handleConvergence() handler
        /// </summary>
        public double mu = 0.44;
        private double initialMu = 0.44;
        private double finalMu = 0.0;
        private double muInfl = 0.4;
        private double muSigma = 0.0001;

        /// <summary>
        /// Learning rate for lateral connections. If > 0.1 then the learning happens in handleConvergence() handler
        /// </summary>
        public double etha = 0.62;
        private double initialEtha = 0.62;
        private double finalEtha = 0.0;
        private double ethaInfl = 0.8;
        private double ethaSigma = 0.04;

        #endregion
        /// <summary>
        /// The neural grid
        /// </summary>
        Neuron[,] map;

        /// <summary>
        /// Hold the local winers
        /// </summary>
        List<Neuron> winners;

        /// <summary>
        /// A neural representation of the modalities
        /// </summary>
        Dictionary<Signal, Neuron[,]> neuralModalities;

        /// <summary>
        /// Just to keep track of which modality holds which input neuron
        /// </summary>
        Dictionary<Neuron, Signal> neuronModalParentModality;

        /// <summary>
        /// Keep track of the specific coordinates of a modal neuron (NOT FOR THE GRID)
        /// </summary>
        Dictionary<Neuron, Point2D> neuronModalCoordinates;

        /// <summary>
        /// The connections between the neural modalities and the grid
        /// </summary>
        Dictionary<Signal, List<Connection> > afferentConnections;

        /// <summary>
        /// The radius of lateral connections
        /// </summary>
        double lateralConnectivityRadius;

        /// <summary>
        /// The lateral connections between neurons of the grid
        /// </summary>
        List<Connection> lateralConnections;

        public MMNodeMWSOM(Point2D mapDim, double lateralConnectivityRadius = 2.0)
            : base(mapDim)
        {
            //Initialise the modalities neural representations
            neuralModalities = new Dictionary<Signal, Neuron[,]>();
            neuronModalCoordinates = new Dictionary<Neuron, Point2D>();
            neuronModalParentModality = new Dictionary<Neuron, Signal>();
            afferentConnections = new Dictionary<Signal, List<Connection>>();

            //Initialise the map
            map = new Neuron[(int)mapDim.X, (int)mapDim.Y];
            ArrayHelper.ForEach(map, false, (x, y) => { map[x, y] = new Neuron(); });

            //Initialise the lateral connections
            lateralConnections = new List<Connection>();
            this.lateralConnectivityRadius = lateralConnectivityRadius;
            ArrayHelper.ForEach(map, false, (x1, y1) => 
            {             
                ArrayHelper.ForEach(map, false, (x2, y2) => 
                { 
                    double d = MathHelpers.distance(x1,y1,x2,y2, Connectivity.torus,map.GetLength(0), map.GetLength(1));
                    if (d < lateralConnectivityRadius) //d>0 means no self connection
                    {
                        if (d == 0) // Self connection, fixed weight
                            lateralConnections.Add(new Connection(map[x1, y1], map[x2, y2], beta, "lateral"));
                        else
                            lateralConnections.Add(new Connection(map[x1,y1], map[x2,y2],0.0,1.0,"lateral"));
                        lateralConnections.Last().length = d;
                    }
                });
            });

            //Other stuff
            winners = new List<Neuron>();

            //Events
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;
            onEpoch += HandleEpoch;
            onBatchStart += HandleBatchStart;
        }

        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);

            //Create the neural modality
            neuralModalities[s] = new Neuron[s.Width, s.Height];
            ArrayHelper.ForEach(neuralModalities[s], false, (x, y) => 
            { 
                neuralModalities[s][x, y] = new Neuron();
                neuronModalParentModality[neuralModalities[s][x, y]] = s;
                neuronModalCoordinates[neuralModalities[s][x, y]] = new Point2D(x, y);
            });

            //Connect it to the whole grid
            afferentConnections[s] = new List<Connection>();
            ArrayHelper.ForEach(neuralModalities[s], false, (x1, y1) =>
            {
                ArrayHelper.ForEach(map, false, (x2, y2) =>
                {
                    afferentConnections[s].Add(new Connection(neuralModalities[s][x1, y1], map[x2, y2], 0.0, 1.0, "afferent"));
                });
            });
        }


        /// <summary>
        /// Implementation of the convergence operation.
        /// </summary>
        protected override void converge()
        {
            //Set the input neural activity
            foreach(Signal s in modalities)
            {
                ArrayHelper.ForEach(neuralModalities[s], true, (x1, y1) =>
                {
                    neuralModalities[s][x1, y1].activity = s.reality[x1, y1];
                });
            }

            //Sum the influences
            double totalInfluence = 0.0;
            foreach (Signal s in modalities)
                totalInfluence += modalitiesInfluence[s];

            //Compute the map inputs
            for (int relaxationT = 0; relaxationT < tau; relaxationT++)
            {
                //Zero the input current & activity
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    map[x, y].inputCurrent = 0.0;
                    if (relaxationT == 0) // Only input from afferent at t0
                        map[x, y].previousActivity = 0.0;
                    else
                        map[x, y].previousActivity = map[x, y].activity;
                    map[x, y].activity = 0.0;
                });

                //Propagate the input current
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    //if (relaxationT == 0) // Only input from afferent at t0
                    {
                        foreach (Connection c in map[x, y].inputs["afferent"])
                        {
                            Signal pMod = neuronModalParentModality[c.source];
                            double influenceFactor = modalitiesInfluence[pMod] / (pMod.Size.X * pMod.Size.Y * totalInfluence);
                            map[x, y].inputCurrent += influenceFactor * alpha * c.weight * c.source.activity;
                            //map[x, y].inputCurrent += influenceFactor * alpha * (1.0 - Math.Pow(c.weight - c.source.activity,2.0));
                        }
                    }

                    //Lateral at every relaxation step
                    foreach (Connection c in map[x, y].inputs["lateral"])
                    {
                        map[x, y].inputCurrent += (1 - alpha) * c.weight * c.source.previousActivity;
                    }
                });

                //Detect the local winners
                winners.Clear();
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    bool isWinner = true;
                    foreach (Connection c in map[x, y].inputs["lateral"])
                    {
                        if (c.source!=c.target && map[x, y].inputCurrent < c.source.inputCurrent)
                            isWinner = false;
                    }
                    if (isWinner)
                        winners.Add(map[x, y]);
                });


                //Compute the activity
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    foreach (Connection c in map[x, y].inputs["lateral"])
                    {
                        if (winners.Contains(c.source))
                            map[x, y].activity += c.source.inputCurrent * Math.Pow(gamma, c.length);
                    }
                    map[x, y].activity = Math.Min(1.0, map[x, y].activity);
                    //MathHelpers.Clamp(ref map[x, y].activity, 0.0, 1.0);
                });

                //Forces the propagation of a convergence event to call learning of HandleConvergence and GUI related stuff.
                //To make it more efficient we could just call HandleConvergence manually here.
                //if (onConvergence != null)
                //    onConvergence();
                //THIS DOES NOT WORK, onConvergence can only be called in the mother class... :-/
                HandleConvergence(this, null);  
            }
            //Copy the map activity to the output of the node
            ArrayHelper.ForEach(map, true, (x, y) => { output.prediction[x, y] = map[x, y].activity; });      
        }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected override void diverge() 
        {
            //Zero the prediction
            Dictionary<Signal, double[,]> contribution = new Dictionary<Signal, double[,]>();
            foreach (Signal s in modalities)
            {
                ArrayHelper.ForEach(neuralModalities[s], true, (x1, y1) =>
                {
                    s.prediction[x1, y1] = 0.0;
                });
                contribution[s] = s.prediction.Clone() as double[,];
            }

            //Sum the winners predictions
            foreach (Neuron win in winners)
            {
                foreach (Connection c in win.inputs["afferent"])
                {
                    Signal srcMod = neuronModalParentModality[c.source];
                    Point2D srcCoo = neuronModalCoordinates[c.source];

                    srcMod.prediction[(int)srcCoo.X, (int)srcCoo.Y] += win.activity * c.weight;
                    contribution[srcMod][(int)srcCoo.X, (int)srcCoo.Y] += win.activity;
                }
            }

            //Average over winners
            foreach (Signal s in modalities)
            {
                ArrayHelper.ForEach(neuralModalities[s], false, (x1, y1) =>
                {
                    if (contribution[s][x1, y1] == 0.0)
                        throw new Exception("Damn");
                    s.prediction[x1, y1] /= contribution[s][x1, y1];
                    //s.prediction[x1, y1] /= winners.Count;
                });
            }
        }

        /// <summary>
        /// Triggered after a convergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleConvergence(object o, EventArgs nullargs)
        {
            //LEARNING happens here
            if ( !learningLocked && ( mu > 0.0 || etha > 0.0) )
            {
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    //Train the afferent connections
                    if (mu > 0.0)
                    {
                        //MyModif
                        double norm = 0.0;
                        foreach (Connection c in map[x, y].inputs["afferent"])
                        {
                            double modInfluence = modalitiesInfluence[neuronModalParentModality[c.source]];
                            c.weight += modInfluence * mu * map[x, y].activity * c.source.activity;
                            norm += c.weight;
                        }

                        if (norm == 0)
                        {
                            throw new Exception("Norm == 0");
                        }
                        //Normalize the afferant weight vector
                        // /!!!!\ this could be done separately for each modality. What are the implications ?
                        foreach (Connection c in map[x, y].inputs["afferent"])
                        {
                            c.weight /= norm;
                        }
                        //foreach (Connection c in map[x, y].inputs["afferent"])
                        //{
                        //    c.weight += 0.01 * map[x, y].activity * (c.source.activity - c.weight);
                        //}
                    }
                    //Train the lateral connections
                    if (etha > 0.0)
                        foreach (Connection c in map[x, y].inputs["lateral"])
                        {
                            if (c.source != c.target) //self connection are not subject to learning
                            {
                                c.weight += etha * c.source.previousActivity * Math.Max(map[x, y].activity - map[x, y].previousActivity, 0.0);
                            }
                        }
                });

            }
        }

        /// <summary>
        /// Triggered after a divergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleDivergence(object o, EventArgs nullargs)
        {            

        }

        /// <summary>
        /// Triggered after an epoch as been run during a batch call.
        /// Adapts the neighborhood radius and learning rate
        /// </summary>
        /// <param name="currentEpoch"> the current epoch</param>
        /// <param name="maximumEpoch"> the maximum epoch number to run</param>
        /// <param name="modalitiesMSE"> the MSE for each modality</param>
        /// <param name="MSE"> the average MSE</param>
        public void HandleEpoch(int currentEpoch, int maximumEpoch, Dictionary<Signal, double> modalitiesMSE, double MSE)
        {
            double phi = currentEpoch / (double)maximumEpoch;
            etha = finalEtha + (initialEtha - finalEtha) / (1 + Math.Exp((phi - ethaInfl) / ethaSigma));
            gamma = finalGamma + (initialGamma - finalGamma ) / (1 + Math.Exp((phi - gammaInfl) / gammaSigma));
            mu = finalMu + (initialMu - finalMu) / (1 + Math.Exp((phi - muInfl) / muSigma));
        }        
        
        /// <summary>
        /// Triggered when a batch is started.
        /// In this case just store the initial parameters
        /// </summary>
        void HandleBatchStart(int maximumEpoch, double MSEStopCriterium)
        {
            etha = initialEtha;
            gamma = initialGamma;
            mu = initialMu;
        }
    }
}
