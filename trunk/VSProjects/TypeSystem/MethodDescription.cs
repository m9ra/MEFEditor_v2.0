using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class MethodDescription
    {
        public readonly InstanceInfo ReturnType;
        public readonly ParameterTypeInfo[] Parameters;
        public readonly bool IsStatic;

        public MethodDescription(InstanceInfo returnType, bool isStatic, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
        }

        public TypeMethodInfo CreateInfo(string methodPath)
        {
            var methodName = nameFrom(methodPath);
            var typeName = typeFrom(methodPath);

            var declaringType = new InstanceInfo(typeName);

            var isGeneric = methodPath.Contains('<');

            return new TypeMethodInfo(declaringType, methodName, ReturnType, Parameters, IsStatic, isGeneric);
        }

        private string nameFrom(string path)
        {
            return path.Split('.').Last();
        }

        private string typeFrom(string path)
        {
            var parts = path.Split('.');
            if (parts.Length < 2)
                throw new NotSupportedException("Invalid path");

            return string.Join(".", parts.Take(parts.Length - 1));
        }

        public static MethodDescription Create<T>(bool isStatic, params ParameterTypeInfo[] parameters)
        {
            return new MethodDescription(InstanceInfo.Create<T>(), isStatic, parameters);
        }

        public static MethodDescription CreateInstance<T>(params ParameterTypeInfo[] parameters)
        {
            return Create<T>(false, parameters);
        }

        public static MethodDescription CreateStatic<T>(params ParameterTypeInfo[] parameters)
        {
            return Create<T>(true, parameters);
        }
    }
}
