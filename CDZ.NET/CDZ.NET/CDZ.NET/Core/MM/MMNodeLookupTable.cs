using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CDZNET.Helpers;

namespace CDZNET.Core
{
    /// <summary>
    /// A multimodal node template.
    /// </summary>
    public class MMNodeLookupTable : MMNode
    {
        public double TRESHOLD_SIMILARITY = 0.01;
        public double learningRate = 0.01;
        public bool allowTemplateCreation = true;
        public class Template
        {
            public Signal modality;
            public double[,] template;
            public int totalEncounters = 0;
            public Dictionary<Signal, Dictionary<Template, int>> associations = new Dictionary<Signal, Dictionary<Template, int>>();

            public Template(Signal modality, double[,] template)
            {
                this.modality = modality;
                this.template = (double[,])template.Clone();
            }

            /// <summary>
            /// Evaluate the template against an observation
            /// </summary>
            /// <param name="observation">the observation</param>
            /// <returns>a score</returns>
            public double evaluate(double[,] observation)
            {
                double score = 0;
                ArrayHelper.ForEach(observation, false, (x, y) => { score += Math.Abs( observation[x, y] - template[x,y]); });
                return score / (observation.GetLength(0) * observation.GetLength(1));
            }

            /// <summary>
            /// Returns the most often associated template of another modality
            /// </summary>
            /// <param name="s">The target modality</param>
            /// <returns>The best matching template</returns>
            public KeyValuePair<Template, int> findBestAssociation(Signal s)
            {
                KeyValuePair<Template, int> best = new KeyValuePair<Template, int>(null, 0);
                if (!associations.ContainsKey(s))
                    associations[s] = new Dictionary<Template, int>();
                foreach (KeyValuePair<Template, int> t in associations[s])
                {
                    if (best.Key == null || t.Value > best.Value)
                        best = t;
                }
                return best;
            }

            /// <summary>
            /// Returns the most similar associated template of another modality
            /// </summary>
            /// <param name="s">The target modality</param>
            /// <param name="value">The value to be evaluated against</param>
            /// <returns>The best closest template</returns>
            public KeyValuePair<Template, int> findMatchingAssociation(Signal s, double[,] value)
            {
                KeyValuePair<Template, int> best = new KeyValuePair<Template, int>(null, 0);
                if (!associations.ContainsKey(s))
                    associations[s] = new Dictionary<Template, int>();
                foreach (KeyValuePair<Template, int> t in associations[s])
                {
                    double score = t.Key.evaluate(value);
                    if (best.Key == null || score > best.Value)
                        best = t;
                }
                return best;
            }
        }

        class Prediction
        {
            public Template predictor;
            public KeyValuePair<Template, int> mostLikely;
            public KeyValuePair<Template, int> mostClose;
        }

        /// <summary>
        /// Contains all the templates, which in turn contains all their links
        /// </summary>
        Dictionary<Signal, List<Template>> templates = new Dictionary<Signal, List<Template>>();

        /// <summary>
        /// The current best matching templates
        /// </summary>
        Dictionary<Signal, KeyValuePair<Template, double>> bestTemplates = new Dictionary<Signal, KeyValuePair<Template, double>>();

        /// <summary>
        /// Contain for each modality(target), the predictions from every other modality (source)
        /// </summary>
        Dictionary<Signal, Dictionary<Signal, Prediction>> predictions = new Dictionary<Signal,Dictionary<Signal,Prediction>>();

        //Events
        public delegate void RuleViolationHandler(Template sourceTemplate, Template strongestExpectation, Template closestExpectation, Template reality);
        public event RuleViolationHandler onRuleViolation;

        public MMNodeLookupTable(Point2D outputDim)
            : base(outputDim)
        {
            onConvergence += HandleConvergence;
            onDivergence += HandleDivergence;
        }

        public override void addModality(Signal s, string label = null)
        {
            base.addModality(s, label);
            templates[s] = new List<Template>();
        }

        /// <summary>
        /// Implementation of the convergence operation.
        /// </summary>
        protected override void converge() 
        {
            //First find all the best matches on each modality
            bestTemplates.Clear();
            foreach(Signal s in modalities)
            {
                //Check all the existing templates for this modality & select the best one
                Template best = null;
                double bestScore = double.PositiveInfinity;
                foreach(Template t in templates[s])
                {
                    double score = t.evaluate(s.reality);
                    if (best == null || score<bestScore)
                    {
                        best = t;
                        bestScore = score;
                    }
                }
                bestTemplates[s] = new KeyValuePair<Template,double>(best,bestScore);
            }
            
            //Detect the unknown patterns and fill the gaps
            foreach(Signal s in modalities)
            {
                //First check if we have a good enough template
                if (bestTemplates[s].Key == null || (bestTemplates[s].Value > TRESHOLD_SIMILARITY && (!learningLocked && allowTemplateCreation)))
                {
                    //We never encountered this specific modality pattern before.
                    //Create it with empty associations
                    Template newTemplate = new Template(s,s.reality);
                    newTemplate.totalEncounters = 1;
                    templates[s].Add(newTemplate);
                    bestTemplates[s] = new KeyValuePair<Template,double>(newTemplate,0.0);
                }
                else
                {
                    bestTemplates[s].Key.totalEncounters++;
                }
            }            
            
            //make predictions & detect the rule violations 
            predictions.Clear();
            foreach(Signal sTarget in modalities)
            {
                predictions[sTarget] = new Dictionary<Signal, Prediction>();

                //Pass all modalities and detect the rules violations
                foreach (Signal sSource in modalities)
                {
                    //Check all the associations known and select the closest and the strongest
                    KeyValuePair<Template, int> closest = bestTemplates[sSource].Key.findMatchingAssociation(sTarget, sTarget.reality);
                    KeyValuePair<Template, int> strongest = bestTemplates[sSource].Key.findBestAssociation(sTarget);
                    
                    //If we do not have any expectation (means we just created this template)
                    if(closest.Key==null)
                    {
                        //Store the best template as the expectation (it can be also a template just created)
                        bestTemplates[sSource].Key.associations[sTarget][bestTemplates[sTarget].Key] = 1;
                        closest = new KeyValuePair<Template, int>(bestTemplates[sTarget].Key, 1);
                        strongest = closest;
                    }
                    else //we do have prediction on the modality s2
                    {
                        //We have already encountered this association
                        if (closest.Key == bestTemplates[sTarget].Key)
                        {
                            //We increment the encounters counter
                            bestTemplates[sSource].Key.associations[sTarget][bestTemplates[sTarget].Key] += 1;
                        }
                        else 
                        {
                            //our closest prediction does not match what we see now. 
                            if (!learningLocked && learningRate>0)
                            {
                                //Build the association
                                bestTemplates[sSource].Key.associations[sTarget][bestTemplates[sTarget].Key] = 1;
                            }
                            closest = new KeyValuePair<Template, int>(bestTemplates[sTarget].Key, 1);

                            //Propagate a rule violation signal (is it ?)
                            if (onRuleViolation != null)
                                onRuleViolation(bestTemplates[sSource].Key, strongest.Key, closest.Key, bestTemplates[sTarget].Key);
                        }
                    }

                    //Create the prediction
                    predictions[sTarget][sSource] = new Prediction();
                    predictions[sTarget][sSource].predictor = bestTemplates[sSource].Key;
                    predictions[sTarget][sSource].mostClose = closest;
                    predictions[sTarget][sSource].mostLikely = strongest;
                }
            }
        }

        /// <summary>
        /// Implementation of the divergence operation.
        /// </summary>
        protected override void diverge() 
        {
            //Passes all the predictions & cook a mixture
            foreach (Signal sTarget in modalities)
            {
                //Zero prediction
                ArrayHelper.ForEach(sTarget.prediction, true, (x, y) => { sTarget.prediction[x, y] = 0.0; });
                double[,] contributions = sTarget.prediction.Clone() as double[,];

                foreach (Signal sSource in modalities)
                {
                    //if (sSource != sTarget)
                    {
                        //First we check if we are not completely wrong about how we represent this modality
                        double confidence = bestTemplates[sSource].Value; //(This varies between [0,1])

                        ArrayHelper.ForEach(sTarget.prediction, false, (x, y) =>
                        {
                            int encounters = predictions[sTarget][sSource].mostLikely.Value;
                            double trustFactor = (1-confidence) * encounters * modalitiesInfluence[sSource];
                            sTarget.prediction[x, y] += trustFactor * predictions[sTarget][sSource].mostLikely.Key.template[x, y];
                            contributions[x, y] += trustFactor;

                            encounters = predictions[sTarget][sSource].mostClose.Value;
                            trustFactor = (confidence) * encounters * modalitiesInfluence[sSource];
                            sTarget.prediction[x, y] += trustFactor * predictions[sTarget][sSource].mostClose.Key.template[x, y];
                            contributions[x, y] += trustFactor;                 
                        });
                    }
                }
                ArrayHelper.ForEach(sTarget.prediction, true, (x, y) => { sTarget.prediction[x, y] /= contributions[x, y]; });
            }
        }

        /// <summary>
        /// Triggered after a convergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleConvergence(object o, EventArgs nullargs)
        {

        }

        /// <summary>
        /// Triggered after a divergence operation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="nullargs"></param>
        public void HandleDivergence(object o, EventArgs nullargs)
        {
            if (!learningLocked)
            {
                //We train the system by moving each pattern a bit closer to what we perceived
                foreach (Signal sTarget in modalities)
                {
                    ArrayHelper.ForEach(sTarget.prediction, true, (x, y) =>
                    {
                        double error = sTarget.reality[x, y] - bestTemplates[sTarget].Key.template[x, y];
                        bestTemplates[sTarget].Key.template[x, y] += learningRate * error;
                    });
                }
            }
        }

    }
}
