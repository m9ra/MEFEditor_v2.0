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
            Implemented = TypeDescriptor.NoDescriptors;
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
            var parsedPath = new PathInfo(methodPath);

            var typeArguments = new List<TypeDescriptor>();
            for (int i = 0; i < parsedPath.GenericArgs.Count; ++i)
            {
                var parameterType = TypeDescriptor.GetParameter(i);
                parsedPath.GenericArgs[i] = parameterType.TypeName;
                typeArguments.Add(parameterType);
            }

            var parametrizedPath = parsedPath.CreateName();

            var methodName = nameFrom(parametrizedPath);
            var typeName = typeFrom(parametrizedPath);


            var declaringType = TypeDescriptor.Create(typeName);

            return new TypeMethodInfo(declaringType, methodName, ReturnType, Parameters, IsStatic, typeArguments.ToArray());
        }

        private string nameFrom(string path)
        {
            var lastToken = path.Split('.').Last();

            var genericArgListInddex = lastToken.IndexOf('<');

            if(genericArgListInddex>0)
                lastToken=lastToken.Substring(0, genericArgListInddex);

            return lastToken;
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
