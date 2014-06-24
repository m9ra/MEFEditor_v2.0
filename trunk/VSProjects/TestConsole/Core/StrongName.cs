using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments.Core
{
    class StrongName
    {
        public readonly string Simple;
        public readonly string Full;

        public StrongName(string simple)
        {
            //TODO full name

           Full= Simple = simple;
        }

        public override bool Equals(object obj)
        {
            var other = obj as StrongName;
            if (other == null)
                return false;

            return other.Full == Full && other.Simple==Simple;
        }

        public override int GetHashCode()
        {
            return Full.GetHashCode() + Simple.GetHashCode();
        }
    }
}
