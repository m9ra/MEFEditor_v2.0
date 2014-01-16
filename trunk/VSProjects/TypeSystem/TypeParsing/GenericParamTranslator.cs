using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace TypeSystem.TypeParsing
{
    /// <summary>
    /// Provider of methods element (Return type, parameter,..) type 
    /// </summary>
    /// <param name="method">Method from which element is extracted</param>
    /// <returns>Extracted element type</returns>
    delegate Type MethodTypeProvider(MethodInfo method);

    /// <summary>
    /// Provider of methods element (Parameter,..) type 
    /// </summary>
    /// <param name="method">Method from which element is extracted</param>
    /// <returns>Extracted element type</returns>
    delegate Type MethodBaseTypeProvider(MethodBase method);

    /// <summary>
    /// Translator used for generic parameters of methods (even thos inherited from defining types)
    /// </summary>
    class GenericParamTranslator
    {
        /// <summary>
        /// Type where translated methods are appering
        /// </summary>
        private readonly TypeDescriptor _declaringType;

        /// <summary>
        /// Type arguments availabe in declaring type
        /// </summary>
        private readonly string[] _typeArguments;

        public GenericParamTranslator(TypeDescriptor declaringType)
        {
            _declaringType = declaringType;
            _typeArguments = _declaringType.Arguments.ToArray();
        }

        /// <summary>
        /// Get translated type descriptor of extracted element's type
        /// </summary>
        /// <param name="context">Context method used for element extraction and translation</param>
        /// <param name="typeProvider">Method element extractor</param>
        /// <returns>Translated type descriptor</returns>
        public TypeDescriptor GetTypeDescriptor(MethodInfo context, MethodTypeProvider typeProvider)
        {
            var type = typeProvider(context);
            //determine that parameter comes from method
            var isGenericMethodParameter = type.IsGenericParameter;

            if (typeof(InstanceWrap).IsAssignableFrom(type))
            {
                var genericDefContext = getGenericDefinition(context);
                type = typeProvider(genericDefContext);
            }

            if (type.IsGenericParameter)
            {
                var absPosition = type.GenericParameterPosition;
                if (isGenericMethodParameter)
                    absPosition += _typeArguments.Length;

                if (_typeArguments.Length > absPosition)
                {
                    return TypeDescriptor.Create(_typeArguments[absPosition]);
                }
                else
                {
                    return TypeDescriptor.GetParameter(absPosition);
                }
            }

            var descriptor = getDescriptor(type);
            return descriptor;
        }

        /// <summary>
        /// Get translated type descriptor of extracted element's type
        /// </summary>
        /// <param name="context">Context method used for element extraction and translation</param>
        /// <param name="typeProvider">Method element extractor</param>
        /// <returns>Translated type descriptor</returns>
        public TypeDescriptor GetTypeDescriptorFromBase(MethodBase context, MethodBaseTypeProvider typeProvider)
        {
            if (context is MethodInfo)
            {
                return GetTypeDescriptor(context as MethodInfo, (m) => typeProvider(m));
            }

            throw new NotImplementedException();
        }

        #region Private utilities

        /// <summary>
        /// Get type descriptor available for given type
        /// </summary>
        /// <param name="type">Type which descriptor is recieved</param>
        /// <returns>Type descriptor</returns>
        private TypeDescriptor getDescriptor(Type type)
        {
            return TypeDescriptor.Create(type);
        }

        /// <summary>
        /// Get generic definition for given method if available
        /// </summary>
        /// <param name="method">Method which generic definition is retrived</param>
        /// <returns>Generic definition if availabe, pasted method othewise</returns>
        private MethodInfo getGenericDefinition(MethodInfo method)
        {
            if (method == null)
                return null;

            if (method.DeclaringType.IsGenericType)
            {
                method = findInTypeDefinition(method.DeclaringType.GetGenericTypeDefinition(), method);
            }

            if (!method.IsGenericMethodDefinition)
                return method;

            return method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Find matching method in given typeDefinition to given method
        /// </summary>
        /// <param name="typeDefinition">Type where definition is searched</param>
        /// <param name="method">Searched method</param>
        /// <returns>Matching method in given typeDefinition</returns>
        private MethodInfo findInTypeDefinition(Type typeDefinition, MethodInfo method)
        {
            //TODO find all bindings
            var methods = from
                              m in typeDefinition.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                          where
                             m.Name == method.Name &&
                             m.GetParameters().Length == method.GetParameters().Length

                          select m;

            if (methods.Count() != 1)
                throw new NotSupportedException("Cannot find generic definition for method: " + method.Name);

            return methods.First();
        }

        #endregion
    }
}
