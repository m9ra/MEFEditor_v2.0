using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Description of method tht is used for creating for signature information in form of <see cref="TypeMethodInfo" />.
    /// </summary>
    public class MethodDescription
    {
        /// <summary>
        /// The return type of method.
        /// </summary>
        public readonly TypeDescriptor ReturnType;

        /// <summary>
        /// The parameters of method..
        /// </summary>
        public readonly ParameterTypeInfo[] Parameters;

        /// <summary>
        /// types which methods are implemented by current method.
        /// </summary>
        public readonly IEnumerable<TypeDescriptor> Implemented;

        /// <summary>
        /// Determine whether method is static.
        /// </summary>
        public readonly bool IsStatic;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription" /> class.
        /// </summary>
        /// <param name="returnType">Method return type.</param>
        /// <param name="isStatic">if set to <c>true</c> method is static.</param>
        /// <param name="parameters">The method parameters.</param>
        public MethodDescription(TypeDescriptor returnType, bool isStatic, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Implemented = TypeDescriptor.NoDescriptors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDescription" /> class.
        /// </summary>
        /// <param name="returnType">Method return type.</param>
        /// <param name="isStatic">if set to <c>true</c> method is static.</param>
        /// <param name="parameters">The method parameters.</param>
        /// <param name="implemented">types which methods are implemented by current method.</param>
        public MethodDescription(TypeDescriptor returnType, bool isStatic, IEnumerable<TypeDescriptor> implemented, params ParameterTypeInfo[] parameters)
        {
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Implemented = implemented;
        }

        /// <summary>
        /// Creates the <see cref="TypeMethodInfo"/> signature representation.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <returns>Created TypeMethodInfo.</returns>
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

        /// <summary>
        /// Parses name from given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Parsed name.</returns>
        private string nameFrom(string path)
        {
            var lastToken = path.Split('.').Last();

            var genericArgListInddex = lastToken.IndexOf('<');

            if(genericArgListInddex>0)
                lastToken=lastToken.Substring(0, genericArgListInddex);

            return lastToken;
        }

        /// <summary>
        /// Parses declaring type from given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Parsed type name</returns>
        /// <exception cref="System.NotSupportedException">Invalid path</exception>
        private string typeFrom(string path)
        {
            var parts = path.Split('.');
            if (parts.Length < 2)
                throw new NotSupportedException("Invalid path");

            return string.Join(".", parts.Take(parts.Length - 1));
        }

        /// <summary>
        /// Factory method of static methods.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="isStatic">if set to <c>true</c> created method will be static.</param>
        /// <param name="parameters">The method parameters.</param>
        /// <returns>MethodDescription.</returns>
        public static MethodDescription Create<T>(bool isStatic, params ParameterTypeInfo[] parameters)
        {
            return new MethodDescription(TypeDescriptor.Create<T>(), isStatic, parameters);
        }

        /// <summary>
        /// Factory method.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>MethodDescription.</returns>
        public static MethodDescription CreateInstance<T>(params ParameterTypeInfo[] parameters)
        {
            return Create<T>(false, parameters);
        }

        /// <summary>
        /// Factory method for static methods.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>MethodDescription.</returns>
        public static MethodDescription CreateStatic<T>(params ParameterTypeInfo[] parameters)
        {
            return Create<T>(true, parameters);
        }

        /// <summary>
        /// Factory method for method implementing given types.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns>MethodDescription.</returns>
        public MethodDescription Implements(params Type[] types)
        {
            var implemented = new List<TypeDescriptor>();
            foreach (var type in types)
            {
                implemented.Add(TypeDescriptor.Create(type));
            }
            return new MethodDescription(ReturnType, IsStatic, implemented, Parameters);
        }

        /// <summary>
        /// Factory method for method with given return type.
        /// </summary>
        /// <param name="type">The return type.</param>
        /// <returns>MethodDescription.</returns>
        public MethodDescription WithReturn(Type type)
        {
            var returnInfo = TypeDescriptor.Create(type);
            return WithReturn(returnInfo);
        }

        /// <summary>
        /// Factory method for method with given return type name.
        /// </summary>
        /// <param name="returnTypeName">Name of the return type.</param>
        /// <returns>MethodDescription.</returns>
        public MethodDescription WithReturn(string returnTypeName)
        {
            var returnInfo = TypeDescriptor.Create(returnTypeName);
            return WithReturn(returnInfo);
        }

        /// <summary>
        /// Factory method for method with given return type descriptor.
        /// </summary>
        /// <param name="returnInfo">The return type descriptor.</param>
        /// <returns>MethodDescription.</returns>
        public MethodDescription WithReturn(TypeDescriptor returnInfo)
        {
            return new MethodDescription(returnInfo, IsStatic, Implemented, Parameters);
        }
    }
}
