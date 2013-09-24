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
        public readonly IEnumerable<InstanceInfo> Implemented;
        public readonly bool IsStatic;

        public MethodDescription(InstanceInfo returnType, bool isStatic, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Implemented = new InstanceInfo[0];
        }

        public MethodDescription(InstanceInfo returnType, bool isStatic, IEnumerable<InstanceInfo> implemented, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Implemented = implemented;
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

        public MethodDescription Implements(params Type[] types)
        {
            var implemented = new List<InstanceInfo>();
            foreach (var type in types)
            {
                implemented.Add(new InstanceInfo(type));
            }
            return new MethodDescription(ReturnType, IsStatic, implemented, Parameters);
        }

        public MethodDescription WithReturn(Type type)
        {
            var returnInfo = new InstanceInfo(type);
            return WithReturn(returnInfo);
        }

        public MethodDescription WithReturn(string returnTypeName)
        {
            var returnInfo = new InstanceInfo(returnTypeName);
            return WithReturn(returnInfo);
        }

        public MethodDescription WithReturn(InstanceInfo returnInfo)
        {
            return new MethodDescription(returnInfo, IsStatic, Implemented, Parameters);
        }
    }
}
