using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgePropagation
{
    class Relation
    {
        public string Subject;
        public string Verb;
        public string CompObject;
        public string CompPlace;
        public string CompTime;
        public Relation()
        {
            this.Subject = null;
            this.Verb = null;
            this.CompObject = null;
            this.CompPlace = null;
            this.CompTime = null;
        }

        public Relation(string subject, string verb, string cobject = null, string cplace = null, string ctime = null)
        {
            this.Subject = subject;
            this.Verb = verb;
            this.CompObject = cobject;
            this.CompPlace = cplace;
            this.CompTime = ctime;
        }

        public string asSentence()
        {
            return Subject + " " + Verb + " " + CompObject + " " + CompPlace + " " + CompTime;
        }
    }

    // Custom comparer for the Product class
    class RelationComparer : IEqualityComparer<Relation>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Relation x, Relation y)
        {
            return (
                x.Subject == y.Subject &&
                x.Verb == y.Verb &&
                x.CompObject == y.CompObject &&
                x.CompPlace == y.CompPlace &&
                x.CompTime == y.CompTime
                );
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Relation r)
        {
            //Calculate the hash code for the product.
            return r.Subject.GetHashCode() ^
                r.Verb.GetHashCode() ^
                r.CompObject.GetHashCode() ^
                r.CompPlace.GetHashCode() ^
                r.CompTime.GetHashCode();
        }

    }
}
