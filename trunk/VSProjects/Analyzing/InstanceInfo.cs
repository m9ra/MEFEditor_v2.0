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


        public InstanceInfo(Type type)
        {
            TypeName = type.FullName;
        }

        public static InstanceInfo Create<Type>()
        {
            return new InstanceInfo(typeof(Type));
        }

        public override string ToString()
        {
            return TypeName;
        }
    }
}
