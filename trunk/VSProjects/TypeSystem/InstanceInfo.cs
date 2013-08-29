using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public class InstanceInfo
    {
        public readonly string TypeName;

        public InstanceInfo(string typeName)
        {
            TypeName = typeName;
        }


        public static InstanceInfo Create<Type>()
        {
            return new InstanceInfo(typeof(Type).FullName);
        }

    }
}
