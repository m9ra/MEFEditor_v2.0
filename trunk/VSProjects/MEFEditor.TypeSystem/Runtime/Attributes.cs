using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ParameterTypesAttribute : Attribute
    {
        public readonly IEnumerable<TypeDescriptor> ParameterTypes;

        public ParameterTypesAttribute(params Type[] parameterTypes)
        {
            ParameterTypes = from parameterType in parameterTypes select TypeDescriptor.Create(parameterType);
        }

        public ParameterTypesAttribute(params string[] parameterTypes)
        {
            ParameterTypes = from parameterType in parameterTypes select TypeDescriptor.Create(parameterType);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReturnTypeAttribute : Attribute
    {
        public readonly TypeDescriptor ReturnInfo;

        public ReturnTypeAttribute(Type returnType)
        {
            ReturnInfo = TypeDescriptor.Create(returnType);
        }

        public ReturnTypeAttribute(string returnTypeFullname)
        {
            ReturnInfo = TypeDescriptor.Create(returnTypeFullname);
        }
    }
}
