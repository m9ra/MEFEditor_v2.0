using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class TypeMethodInfo
    {
        public readonly string TypeName;
        public readonly string MethodName;
        public readonly bool IsStatic;
        public readonly ParameterInfo[] Arguments;
        public readonly InstanceInfo ThisType;
        public readonly InstanceInfo ReturnType;

        public bool HasThis { get { return !IsStatic; } }

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

        public TypeMethodInfo(InstanceInfo thisType, string methodName, InstanceInfo returnType, ParameterInfo[] parameters, bool isStatic)
        {
            TypeName = thisType.TypeName;
            MethodName = methodName;
            IsStatic = isStatic;
            Arguments = parameters;
            ThisType = thisType;
            //TODO correct return type
            ReturnType = returnType;
        }        
    }
}
