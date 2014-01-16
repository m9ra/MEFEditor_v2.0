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
        public readonly TypeDescriptor ReturnType;
        public readonly ParameterTypeInfo[] Parameters;
        public readonly IEnumerable<TypeDescriptor> Implemented;
        public readonly bool IsStatic;

        public MethodDescription(TypeDescriptor returnType, bool isStatic, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Implemented = new TypeDescriptor[0];
        }

        public MethodDescription(TypeDescriptor returnType, bool isStatic, IEnumerable<TypeDescriptor> implemented, params ParameterTypeInfo[] parameters)
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

            var declaringType = TypeDescriptor.Create(typeName);

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
            return new MethodDescription(TypeDescriptor.Create<T>(), isStatic, parameters);
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
            var implemented = new List<TypeDescriptor>();
            foreach (var type in types)
            {
                implemented.Add(TypeDescriptor.Create(type));
            }
            return new MethodDescription(ReturnType, IsStatic, implemented, Parameters);
        }

        public MethodDescription WithReturn(Type type)
        {
            var returnInfo = TypeDescriptor.Create(type);
            return WithReturn(returnInfo);
        }

        public MethodDescription WithReturn(string returnTypeName)
        {
            var returnInfo = TypeDescriptor.Create(returnTypeName);
            return WithReturn(returnInfo);
        }

        public MethodDescription WithReturn(TypeDescriptor returnInfo)
        {
            return new MethodDescription(returnInfo, IsStatic, Implemented, Parameters);
        }
    }
}
