using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public class TypeMethodInfo
    {
        public readonly string TypeName;
        public readonly string MethodName;

        public TypeMethodInfo(string typeName, string methodName)
        {
            TypeName = typeName;
            MethodName = methodName;
        }
    }
}
