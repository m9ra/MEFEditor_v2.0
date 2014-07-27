using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.TypeSystem.TypeParsing
{
    public delegate TypeDescriptor ParameterResolver(TypeAdapterBase type);

    public class TypeHierarchyDirector
    {
        private readonly TypeDescriptorBuilder _builder;

        private readonly ParameterResolver _resolver;

        private TypeHierarchyDirector(TypeDescriptorBuilder builder, ParameterResolver resolver)
        {
            _resolver = resolver;
            _builder = builder;
        }

        public static TypeDescriptor BuildDescriptor(TypeAdapterBase type, ParameterResolver resolver, TypeDescriptorBuilder builder = null)
        {
            if (builder == null)
                builder = new TypeDescriptorBuilder();

            var director = new TypeHierarchyDirector(builder, resolver);
            director.buildType(type);

            var result = builder.BuildDescriptor();
            return result;
        }

        /// <summary>
        /// Director for building given type
        /// </summary>
        /// <param name="type">Builded type</param>
        private void buildType(TypeAdapterBase type)
        {
            if (type.IsArray)
            {
                buildArray(type);
            }
            else if (type.IsGenericParameter)
            {
                var substitution = _resolver(type);
                _builder.SetDescriptor(substitution);
            }
            else
            {
                var genericArgs = new Queue<TypeAdapterBase>(type.GenericArgs);
                buildTypeChain(type, genericArgs);
            }
        }

        /// <summary>
        /// Director for building given type chain (DeclaringType chain) with builder
        /// </summary>
        /// <param name="type">Type available for builded subchain</param>
        /// <param name="builder">Builder used by director</param>
        /// <param name="typeArguments">Builded type arguments</param>
        private void buildTypeChain(TypeAdapterBase type, Queue<TypeAdapterBase> typeArguments)
        {
            var declaringType = type.DeclaringType;
            var hasConnectedType = declaringType != null;

            //take as much arguments, as connected types needs + (mine types)
            var parameters = type.GenericArgs;
            var availableArguments = new Queue<TypeAdapterBase>();
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
        /// Director for building given arrayType with builder
        /// </summary>
        /// <param name="type">Builded type</param>
        private void buildArray(TypeAdapterBase type)
        {
            _builder.Append("Array");

            _builder.Push();
            buildType(type.ElementType);
            _builder.Pop();

            //TODO refactor dimension argument handling
            _builder.InsertArgument("1");
        }

        /// <summary>
        /// Director for building given type name with builder
        /// </summary>
        /// <param name="type">Type which name is builded</param>
        private void buildName(TypeAdapterBase type)
        {
            var name = type.Name;
            var endName = name.IndexOf('`');
            if (endName > 0)
                name = name.Substring(0, endName);

            _builder.Append(type.Namespace);
            _builder.Append(name);
        }

        /// <summary>
        /// Director for building given type arguments with builder
        /// </summary>
        /// <param name="type">Type which arguments are builded</param>
        /// <param name="typeArguments">Arguments that are available for building given type</param>
        private void buildArguments(TypeAdapterBase type, Queue<TypeAdapterBase> typeArguments)
        {
            var typeParams = type.GenericArgs;

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
    }
}
