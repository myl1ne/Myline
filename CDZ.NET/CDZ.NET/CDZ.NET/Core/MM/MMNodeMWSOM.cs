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
        /// Tuning factor afferent vs lateral
        /// </summary>
        public double alpha = 0.2;

        /// <summary>
        /// Activation dropoff
        /// </summary>
        public double gamma = 0.37;

        /// <summary>
        /// Learning rate for afferent connections. If > 0.1 then the learning happens in handleConvergence() handler
        /// </summary>
        public double mu = 0.4;

        /// <summary>
        /// Learning rate for lateral connections. If > 0.1 then the learning happens in handleConvergence() handler
        /// </summary>
        public double etha = 0.62;

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
        Dictionary<Neuron, Signal> neuronParentModality;

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

        public MMNodeMWSOM(Point2D mapDim, Point2D outputDim, double lateralConnectivityRadius = 2.0)
            : base(outputDim)
        {
            //Initialise the modalities neural representations
            neuralModalities = new Dictionary<Signal, Neuron[,]>();
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
                    if (d > 0 && d < lateralConnectivityRadius) //d>0 means no self connection
                    {
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
        }

        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);

            //Create the neural modality
            neuralModalities[s] = new Neuron[s.Width, s.Height];
            ArrayHelper.ForEach(neuralModalities[s], false, (x, y) => 
            { 
                neuralModalities[s][x, y] = new Neuron();
                neuronParentModality[neuralModalities[s][x, y]] = s;
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

            //Compute the map inputs
            ArrayHelper.ForEach(map, false, (x, y) =>
            {
                map[x,y].inputCurrent = 0.0;
                foreach(Connection c in map[x,y].inputs["afferent"])
                {
                    map[x, y].inputCurrent += modalitiesInfluence[neuronParentModality[c.source]] * alpha * c.weight * c.source.activity;
                }

                foreach (Connection c in map[x, y].inputs["lateral"])
                {
                    map[x, y].inputCurrent += (1 - alpha) * c.weight * c.source.activity;
                }  
            });

            //Detect the local winners
            winners.Clear();
            ArrayHelper.ForEach(map, false, (x, y) =>
            {
                bool isWinner = true;
                foreach (Connection c in map[x, y].inputs["lateral"])
                {
                    if (map[x, y].inputCurrent <= c.source.inputCurrent)
                        isWinner = false;
                }
                if (isWinner)
                    winners.Add(map[x, y]);
            });

            //Compute the activity
            ArrayHelper.ForEach(map, false, (x, y) =>
            {
                map[x, y].previousActivity = map[x, y].activity;

                if (winners.Contains(map[x, y]))
                    map[x, y].activity = map[x, y].inputCurrent;
                else
                {
                    map[x, y].activity = 0.0;
                    foreach (Connection c in map[x, y].inputs["lateral"])
                    {
                        map[x, y].activity += c.source.inputCurrent * Math.Pow(gamma,c.length);
                    }
                }
            });
        }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected override void diverge() 
        { 
            //??? How to go back ?
        }

        /// <summary>
        /// Triggered after a convergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleConvergence(object o, EventArgs nullargs)
        {
            if (mu>0.0 || etha>0.0)
            {
                ArrayHelper.ForEach(map, false, (x, y) =>
                {
                    //Train the afferent connections
                    if (mu>0.0)
                    foreach (Connection c in map[x, y].inputs["afferent"])
                    {
                        double modInfluence = modalitiesInfluence[neuronParentModality[c.source]];
                        c.weight += modInfluence * mu * map[x, y].activity * c.source.activity;
                    }

                    //Train the lateral connections
                    if (etha > 0.0)
                    foreach (Connection c in map[x, y].inputs["lateral"])
                    {
                        c.weight += etha * c.source.previousActivity * Math.Max(map[x, y].activity - map[x, y].previousActivity, 0.0);
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
    }
}
