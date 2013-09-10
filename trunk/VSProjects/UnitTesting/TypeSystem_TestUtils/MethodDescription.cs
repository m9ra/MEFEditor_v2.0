using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

using Analyzing;

namespace UnitTesting.TypeSystem_TestUtils
{
    public class MethodDescription
    {
        public readonly InstanceInfo ReturnType;
        public readonly TypeParameterInfo[] Parameters;
        public readonly bool IsStatic;

        public MethodDescription(InstanceInfo returnType, bool isStatic, params TypeParameterInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
        }

        internal TypeMethodInfo CreateInfo(string methodPath)
        {
            var methodName = nameFrom(methodPath);
            var typeName = typeFrom(methodPath);

            var declaringType = new InstanceInfo(typeName);

            return new TypeMethodInfo(declaringType, methodName, ReturnType, Parameters, IsStatic);
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

        public static MethodDescription Create<T>(bool isStatic, params TypeParameterInfo[] parameters)
        {
            return new MethodDescription(InstanceInfo.Create<T>(), isStatic, parameters);
        }

        public static MethodDescription CreateInstance<T>(params TypeParameterInfo[] parameters)
        {
            return Create<T>(false, parameters);
        }

        public static MethodDescription CreateStatic<T>(params TypeParameterInfo[] parameters)
        {
            return Create<T>(true, parameters);
        }
    }
}
