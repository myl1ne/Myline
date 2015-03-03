using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgePropagation
{
    class Agent:Entity
    {
        public Dictionary<string, Entity> concepts;
        public List<Relation> beliefs;

        public Agent(string name):base(name)
        {
            concepts = new Dictionary<string,Entity>();
            beliefs = new List<Relation>();
        }

        public bool considerInformation(Relation r, IEnumerable<Agent> presentAgents = null, Agent informator = null)
        {
            //Check if this information is new
            if (beliefs.Contains(r, new RelationComparer()))
            {
                //already knew this fact
                //we just update or relation of the other
                //Console.WriteLine(this.Name + " : I already knew that.");
                if (informator != null)
                    updateOtherRepresentation(informator.Name, r);
                if (presentAgents != null)
                {
                    foreach (Agent a in presentAgents)
                    {
                        if (a != this)
                            updateOtherRepresentation(a.Name, r);
                    }
                }
                return false;
            }

            //Here we could add something about thrust
            Relation mySideRelation = new Relation();

            //Add the concepts that we do not know
            if (!concepts.ContainsKey(r.Subject))
                concepts.Add(r.Subject, new Entity(r.Subject));
            mySideRelation.Subject = concepts[r.Subject].Name;

            if (!concepts.ContainsKey(r.Verb))
                concepts.Add(r.Verb, new Entity(r.Verb));
            mySideRelation.Verb = concepts[r.Verb].Name;

            if (!concepts.ContainsKey(r.CompObject))
                concepts.Add(r.CompObject, new Entity(r.CompObject));
            mySideRelation.CompObject = concepts[r.CompObject].Name;

            if (r.CompPlace != null)
            {
                if (!concepts.ContainsKey(r.CompPlace))
                    concepts.Add(r.CompPlace, new Entity(r.CompPlace));
                mySideRelation.CompPlace = concepts[r.CompPlace].Name;
            }

            if (r.CompTime != null)
            {
                if (!concepts.ContainsKey(r.CompTime))
                    concepts.Add(r.CompTime, new Entity(r.CompTime));
                mySideRelation.CompTime = concepts[r.CompTime].Name;
            }
            //Add the relation as part of our knowledge
            beliefs.Add(mySideRelation);

            //Add this relation as a beliefs of the interlocutor
            if (informator!=null)
                updateOtherRepresentation(informator.Name, r);

            if (presentAgents != null)
            {
                foreach (Agent a in presentAgents)
                {
                    if (a != this)
                        updateOtherRepresentation(a.Name, r);
                }
            }
            return true;
        }

        public void updateOtherRepresentation(string otherName, Relation r)
        {
            if (!concepts.ContainsKey(otherName))
                concepts[otherName] = new Agent(otherName);

            Agent other = concepts[otherName] as Agent;
            if (!other.concepts.ContainsKey(r.Subject))
                other.concepts.Add(r.Subject, new Entity(r.Subject));

            if (!other.concepts.ContainsKey(r.Verb))
                other.concepts.Add(r.Verb, new Entity(r.Verb));

            if (!other.concepts.ContainsKey(r.CompObject))
                other.concepts.Add(r.CompObject, new Entity(r.CompObject));

            if (r.CompPlace!=null && !other.concepts.ContainsKey(r.CompPlace))
                other.concepts.Add(r.CompPlace, new Entity(r.CompPlace));

            if (r.CompTime != null && !other.concepts.ContainsKey(r.CompTime))
                other.concepts.Add(r.CompTime, new Entity(r.CompTime));

            other.beliefs.Add(r);
        }

        public bool inform(Agent a, bool USE_OTHER_MODEL)
        {
            //Check if we already met this guy
            if (!concepts.ContainsKey(a.Name))
                concepts[a.Name] = new Agent(a.Name);

            //Randomize our beliefs
            Random rnd = new Random();
            IEnumerable<Relation> shuffledBeliefs = beliefs.OrderBy( (r) => { return rnd.Next(); });

            //Tell him something
            Agent interlocutor = concepts[a.Name] as Agent;
            RelationComparer rComp = new RelationComparer();

            foreach (Relation r in shuffledBeliefs)
            {
                if (USE_OTHER_MODEL)
                {
                    if (!interlocutor.beliefs.Contains(r, rComp))
                    {
                        //We can say something that he doesn't know
                        Console.WriteLine(this.Name + " think & says to " + interlocutor.Name + " : " + r.asSentence());
                        a.considerInformation(r, new List<Agent>(), this);
                        //We consider that now he knows
                        updateOtherRepresentation(interlocutor.Name, r);
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine(this.Name + " says to " + interlocutor.Name + " : " + r.asSentence());
                    a.considerInformation(r,new List<Agent>(), this);
                    //We consider that now he knows
                    updateOtherRepresentation(interlocutor.Name, r);
                    return true;
                }
            }

            //Nothing to say
            Console.WriteLine(this.Name + " think & and has nothing to say to " + interlocutor.Name);
            return false;
        }


        public bool inform(IEnumerable<Agent> group, bool USE_OTHER_MODEL)
        {
            //Check if we already met this guy
            foreach (Agent a in group)
            {
                if (!concepts.ContainsKey(a.Name))
                    concepts[a.Name] = new Agent(a.Name);
            }

            //Rank our beliefs according to how rarely they are spread around    
            RelationComparer rComp = new RelationComparer();
            Random rnd = new Random();
            Dictionary<Relation, int > rankedBeliefs = new  Dictionary<Relation,int>();
            Relation bestRelation = null;
            foreach(Relation r in beliefs)
            {
                rankedBeliefs[r] = 0;

                if (USE_OTHER_MODEL)
                {
                    foreach (Agent a in group)
                    {
                        Agent otherRepresentation = concepts[a.Name] as Agent;
                        if (!otherRepresentation.beliefs.Contains(r, rComp))
                            rankedBeliefs[r]++;
                    }
                }
                else
                {
                    rankedBeliefs[r] = rnd.Next();
                }

                if (bestRelation == null || rankedBeliefs[bestRelation] < rankedBeliefs[r])
                    bestRelation = r;
            }


            //Tell the relation that is the less know
            if (rankedBeliefs[bestRelation] != 0)
            {
                //Console.WriteLine(this.Name + " hope to inform " + rankedBeliefs[bestRelation] + " agents by saying " + bestRelation.asSentence());
                int reallyInformed = 0;
                foreach (Agent a in group)
                {
                    Agent otherRepresentation = concepts[a.Name] as Agent;
                    //We can say something that he doesn't know
                    if (a.considerInformation(bestRelation, group, this))
                        reallyInformed++;

                    //We consider that now he knows
                    updateOtherRepresentation(otherRepresentation.Name, bestRelation);
                }
                //Console.WriteLine(this.Name + "really informed " + reallyInformed + " agents");
            }
            else
            {
                //Nothing to say
                //Console.WriteLine(this.Name + " think & and has nothing to say");
            }
            return false;
        }
    }
}
