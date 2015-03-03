using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace KnowledgePropagation
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();

            StreamWriter file = new StreamWriter("knowledgePropagation.csv");
            file.WriteLine("run,agentCount,groupSize,otherMdl,t,agentName,beliefsCount,%ofKB");

            Random rnd = new Random();
            for (int run = 0; run < 10; run++)
            {
                Console.WriteLine("Run = " + run);
                for (int agentsCount = 16; agentsCount < 128; agentsCount *= 2)
                {
                    Console.WriteLine("\t Agents = " + agentsCount);
                    for (int groupSize = 2; groupSize <= agentsCount; groupSize *= 2)
                    {
                        Console.WriteLine("\t \t GrpSize = " + groupSize);

                        int totalObjects = 100;
                        int totalVerbs = 25;
                        //int agentsCount = 9;
                        int relationPerAgent = 10;
                        int groupNumber = agentsCount / groupSize;

                        //------------------------------------Create knowledge pool----------------------------
                        List<string> objects = new List<string>();

                        for (int i = 0; i < totalObjects; i++)
                        {
                            objects.Add("Object_" + i);
                        }

                        List<string> verbs = new List<string>();

                        for (int i = 0; i < totalVerbs; i++)
                        {
                            verbs.Add("Verb_" + i);
                        }

                        //--------------------------------------Create agents-----------------------
                        List<Agent> agents = new List<Agent>();
                        List<Relation> allKnowledge = new List<Relation>();
                        Dictionary<Agent, List<Relation>> initialKnowledge = new Dictionary<Agent, List<Relation>>();

                        for (int i = 0; i < agentsCount; i++)
                        {
                            Agent a = new Agent("Agent_" + i);
                            agents.Add(a);
                            initialKnowledge[a] = new List<Relation>();

                            //Create his knowledge
                            for (int j = 0; j < relationPerAgent; j++)
                            {
                                Relation r = new Relation(objects[rnd.Next(objects.Count)], verbs[rnd.Next(verbs.Count)], objects[rnd.Next(objects.Count)]);
                                initialKnowledge[a].Add(r);
                                //a.considerInformation(r);
                                if (!allKnowledge.Contains(r, new RelationComparer()))
                                    allKnowledge.Add(r);
                            }
                        }

                        for (int otherModel = 0; otherModel <= 1; otherModel++)
                        {
                            //Reinitialise knowlege
                            foreach (Agent a in agents)
                            {
                                a.beliefs.Clear();
                                a.concepts.Clear();
                                foreach (Relation r in initialKnowledge[a])
                                {
                                    a.considerInformation(r);
                                }
                            }

                            for (int t = 0; t < 500; t++)
                            {
                                //Check the status before speaking
                                int meanRelationKnown = 0;
                                foreach (Agent a in agents)
                                {
                                    file.WriteLine(
                                        run + "," +
                                        agentsCount + "," +
                                        groupSize + "," +
                                        (otherModel == 1) + "," +
                                        t + "," +
                                        a.Name + "," +
                                        a.beliefs.Count + "," +
                                        a.beliefs.Count / (double)allKnowledge.Count);

                                    meanRelationKnown += a.beliefs.Count;
                                }

                                meanRelationKnown /= agents.Count;
                                if (meanRelationKnown == allKnowledge.Count)
                                {
                                    // file.Close();
                                    //Console.WriteLine("All knowledge spreaded in " + t + " cycles.");
                                    //Console.ReadKey();
                                }
                                else
                                {
                                    List<List<Agent>> groups = split(agents, groupNumber);

                                    Parallel.For(0, groupNumber, g =>
                                    {
                                        //Console.WriteLine("Processing group " + g);
                                        foreach (Agent a in groups[g])
                                        {
                                            a.inform(groups[g].Where(person => person != a), otherModel == 1);
                                        }
                                    });
                                }
                                //Console.WriteLine("-----------------------");
                            }
                        }
                        file.Flush();
                    }
                }
            }
            file.Close();
        }

        List< List<Agent> > split(List<Agent> toSplit, int numGroups)
        {
            List<Agent> remainingAgents = new List<Agent>(toSplit);
            Random rnd = new Random();
            List<List<Agent>> groups = new List<List<Agent>>();
            for (int i = 0; i < numGroups; i++)
            {
                List<Agent> group = new List<Agent>();

                for (int a = 0; a < toSplit.Count / numGroups && remainingAgents.Count >0; a++)
                {
                    Agent toAdd = remainingAgents[rnd.Next(remainingAgents.Count)];
                    group.Add(toAdd);
                    remainingAgents.Remove(toAdd);
                }

                groups.Add(group);
            }
            return groups;
        }
    }
}
