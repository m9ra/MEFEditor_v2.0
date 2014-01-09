using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class InstanceInfo
    {
        public readonly string TypeName;

        public InstanceInfo(string typeName)
        {
            if (typeName == null || typeName == "")
            {
                throw new NotSupportedException("Unsupported typename: " + typeName);
            }
            TypeName = typeName;
        }

        public override string ToString()
        {
            return "[InstanceInfo]" + TypeName;
        }


        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as InstanceInfo;
            if (o == null)
            {
                return false;
            }
            return TypeName.Equals(o.TypeName);
        }
    }
}
