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
        public readonly bool IsStatic;
        public readonly ParameterInfo[] Arguments;
        public readonly InstanceInfo ThisType;

        public string Path
        {
            get
            {
                if (TypeName != "")
                {
                    return TypeName + "." + MethodName;
                }
                else
                {
                    return MethodName;
                }
            }
        }

        public TypeMethodInfo(string typeName, string methodName,ParameterInfo[]arguments,bool isStatic)
        {
            TypeName = typeName;
            MethodName = methodName;
            IsStatic = isStatic;
            Arguments = arguments;
            ThisType = new InstanceInfo(TypeName);
        }

        
    }
}
