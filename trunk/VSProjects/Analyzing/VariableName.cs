using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class VariableName
    {
        public readonly string Name;

        public VariableName(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var o = obj as VariableName;
            if (o == null)
            {
                return false;
            }

            return o.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("variable: {0}", Name);
        }
    }
}
