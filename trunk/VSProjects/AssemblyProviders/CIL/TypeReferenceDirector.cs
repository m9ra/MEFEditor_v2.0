using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using TypeSystem;
using TypeSystem.TypeParsing;

namespace AssemblyProviders.CIL
{
    /// <summary>
    /// TODO THIS HAS TO BE REFACTORED (almost identical way of type visiting as in TypeDescriptor.Create
    /// </summary>
    class TypeReferenceDirector
    {
        internal readonly Dictionary<GenericParameter, TypeDescriptor> Substitutions = new Dictionary<GenericParameter, TypeDescriptor>();

        private TypeDescriptorBuilder _builder;

        internal TypeDescriptor Build(TypeReference type)
        {
            _builder = new TypeDescriptorBuilder();
            buildType(type);
            return _builder.BuildDescriptor();
        }

        private void buildType(TypeReference type)
        {
            if (type.IsArray)
            {
                buildArray(type);
            }
            else if (type.IsGenericParameter)
            {
                var parameterType = type as GenericParameter;
                var substitution = resolveSubstitution(parameterType);
                _builder.SetDescriptor(substitution);
            }
            else
            {
                var genericArgs = new Queue<TypeReference>(getGenericArgs(type));
                buildTypeChain(type, genericArgs);
            }
        }

        private TypeDescriptor resolveSubstitution(GenericParameter parameterType)
        {
            TypeDescriptor result;
            if (!Substitutions.TryGetValue(parameterType, out result))
            {
                result = TypeDescriptor.GetParameter(Substitutions.Count); //TODO determine correct ordering
                Substitutions[parameterType] = result;
            }

            return result;
        }

        private void buildTypeChain(TypeReference type, Queue<TypeReference> typeArguments)
        {
            var declaringType = type.DeclaringType;
            var hasConnectedType = declaringType != null;

            //take as much arguments, as connected types needs + (mine types)
            var parameters = getGenericArgs(type);
            var availableArguments = new Queue<TypeReference>();
            for (int i = 0; i < parameters.Length; ++i)
            {
                availableArguments.Enqueue(typeArguments.Dequeue());
            }

            if (hasConnectedType)
            {
                buildTypeChain(declaringType, availableArguments);
                _builder.Push();
            }

            buildName(type);
            buildArguments(type, availableArguments);

            if (hasConnectedType)
            {
                _builder.ConnectPop();
            }
        }

        /// <summary>
        /// Director for building given type arguments with builder
        /// </summary>
        /// <param name="type">Type which arguments are builded</param>
        /// <param name="builder">Builder used by director</param>
        /// <param name="typeArguments">Builded type arguments</param>
        private void buildArguments(TypeReference type, Queue<TypeReference> typeArguments)
        {
            var typeParams = getGenericArgs(type);

            for (int i = 0; i < typeParams.Length; ++i)
            {
                if (typeArguments.Count == 0)
                {
                    //all arguments has already been substituted
                    return;
                }

                //take only fist params  that will be substituted
                var substitution = typeArguments.Dequeue();

                _builder.Push();
                buildType(substitution);
                _builder.Pop();
            }
        }

        private void buildArray(TypeReference type)
        {
            _builder.Append("Array");
            _builder.Push();

            buildType(type.GetElementType());
            _builder.Pop();

            //TODO refactor dimension argument handling
            _builder.InsertArgument("1");
        }



        private TypeReference[] getGenericArgs(TypeReference type)
        {
            var genericInstance = type as GenericInstanceType;

            if (genericInstance == null)
                return type.GenericParameters.ToArray();

            return genericInstance.GenericArguments.ToArray();
        }

        /// <summary>
        /// Director for building given type name with builder
        /// </summary>
        /// <param name="type">Type which name is builded</param>
        /// <param name="builder">Builder used by director</param>
        private void buildName(TypeReference type)
        {
            var name = type.Name;
            var endName = name.IndexOf('`');
            if (endName > 0)
                name = name.Substring(0, endName);

            _builder.Append(type.Namespace);
            _builder.Append(name);
        }
    }
}
