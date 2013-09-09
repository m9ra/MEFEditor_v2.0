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
        public readonly string MethodName;
        public readonly bool IsStatic;
        public readonly ParameterInfo[] Parameters;
        public readonly InstanceInfo DeclaringType;
        public readonly InstanceInfo ReturnType;
        public readonly MethodID MethodID;

        public bool HasThis { get { return !IsStatic; } }

        public string Path
        {
            get
            {
                var typeName = DeclaringType.TypeName;
                if (typeName != "")
                {
                    return typeName + "." + MethodName;
                }
                else
                {
                    return MethodName;
                }
            }
        }

        public TypeMethodInfo(InstanceInfo declaringType, string methodName, InstanceInfo returnType, ParameterInfo[] parameters, bool isStatic)
        {
            if (declaringType == null)
                throw new ArgumentNullException("thisType");

            DeclaringType = declaringType;

            MethodName = methodName;
            IsStatic = isStatic;
            Parameters = parameters;                        
            ReturnType = returnType;
            MethodID = Naming.Method(declaringType,methodName,parameters);
        }        
    }
}
