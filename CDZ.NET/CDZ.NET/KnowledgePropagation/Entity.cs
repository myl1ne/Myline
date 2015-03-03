using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgePropagation
{
    class Entity
    {
        public string Name;

        public Entity(string name)
        {
            this.Name = name;
        }

        public static Entity Factory(Entity toClone)
        {
            Type t = toClone.GetType();

            if (t == typeof(Entity))
                return new Entity(toClone.Name);

            if (t == typeof(Agent))
                return new Agent(toClone.Name);

            return null;
        }
    }
}
