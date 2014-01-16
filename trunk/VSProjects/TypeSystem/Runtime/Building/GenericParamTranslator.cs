using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace TypeSystem.Runtime.Building
{
    delegate Type MethodTypeProvider(MethodInfo method);

    delegate Type MethodBaseTypeProvider(MethodBase method);

    class GenericParamTranslator
    {
        private readonly TypeDescriptor _declaringType;

        private readonly string[] _arguments;

        public GenericParamTranslator(TypeDescriptor declaringType)
        {
            _declaringType = declaringType;
            _arguments = _declaringType.Arguments.ToArray();
        }
        public TypeDescriptor GetTypeDescriptor(MethodInfo context, MethodTypeProvider typeProvider)
        {
            var type = typeProvider(context);
            var isMethodGeneric = type.IsGenericParameter;

            if (typeof(InstanceWrap).IsAssignableFrom(type))
            {
                //TODO determine that parameter comes from type or method
                var genericDefContext = getGenericDefinition(context);
                type = typeProvider(genericDefContext);
            }

            if (type.IsGenericParameter)
            {
                //TODO mark descriptor as parameter
                var absPosition = type.GenericParameterPosition;
                if (isMethodGeneric)
                    absPosition += _arguments.Length;

                if (_arguments.Length > absPosition)
                {
                    return TypeDescriptor.Create(_arguments[absPosition]);
                }
                else
                {
                    return TypeDescriptor.GetParameter(absPosition);
                }
            }

            var descriptor = getDescriptor(type);
            return descriptor;
        }

        public TypeDescriptor GetTypeDescriptorFromBase(MethodBase context, MethodBaseTypeProvider typeProvider)
        {
            if (context is MethodInfo)
            {
                return GetTypeDescriptor(context as MethodInfo, (m) => typeProvider(m));
            }

            throw new NotImplementedException();
        }

        private TypeDescriptor getDescriptor(Type type)
        {
            return TypeDescriptor.Create(type);
        }

        private MethodInfo getGenericDefinition(MethodInfo method)
        {
            if (method == null)
                return null;

            if (method.DeclaringType.IsGenericType)
            {
                method = findGenericMatch(method.DeclaringType.GetGenericTypeDefinition(), method);
            }

            if (!method.IsGenericMethodDefinition)
                return method;

            return method.GetGenericMethodDefinition();
        }

        private MethodInfo findGenericMatch(Type type, MethodInfo method)
        {
            //TODO find all bindings
            var methods = from
                              m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                          where
                             m.Name == method.Name &&
                             m.GetParameters().Length == method.GetParameters().Length

                          select m;

            if (methods.Count() != 1)
                throw new NotSupportedException("Cannot find generic definition for method: " + method.Name);

            return methods.First();
        }
    }
}
