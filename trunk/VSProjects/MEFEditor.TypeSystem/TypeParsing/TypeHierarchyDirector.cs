using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.TypeSystem.TypeParsing
{

    /// <summary>
    /// Resolver of generic parameters of adapted type.
    /// </summary>
    /// <param name="type">The adapted type.</param>
    /// <returns>Resolved parameter.</returns>
    public delegate TypeDescriptor ParameterResolver(TypeAdapterBase type);


    /// <summary>
    /// Director for traversing type hierarchy. It is used
    /// for adapted type system conversion. Type adapting
    /// can be done by <see cref="TypeAdapterBase" />.
    /// </summary>
    public class TypeHierarchyDirector
    {
        /// <summary>
        /// Builder of type descriptors.
        /// </summary>
        private readonly TypeDescriptorBuilder _builder;

        /// <summary>
        /// Resolver of type parameters.
        /// </summary>
        private readonly ParameterResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHierarchyDirector" /> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="resolver">The resolver.</param>
        private TypeHierarchyDirector(TypeDescriptorBuilder builder, ParameterResolver resolver)
        {
            _resolver = resolver;
            _builder = builder;
        }

        /// <summary>
        /// Builds the descriptor for given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="resolver">The generic parameter resolver.</param>
        /// <param name="builder">The <see cref="TypeDescriptor"/> builder.</param>
        /// <returns>Built descriptor.</returns>
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
        /// Director for building given type.
        /// </summary>
        /// <param name="type">Built type.</param>
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
        /// Director for building given type chain (DeclaringType chain) with builder.
        /// </summary>
        /// <param name="type">Type available for built subchain.</param>
        /// <param name="typeArguments">Built type arguments.</param>
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
        /// Director for building given arrayType with builder.
        /// </summary>
        /// <param name="type">Built type.</param>
        private void buildArray(TypeAdapterBase type)
        {
            _builder.Append("Array");

            _builder.Push();
            buildType(type.ElementType);
            _builder.Pop();

            _builder.InsertArgument("1");
        }

        /// <summary>
        /// Director for building given type name with builder.
        /// </summary>
        /// <param name="type">Type which name is built.</param>
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
        /// Director for building given type arguments with builder.
        /// </summary>
        /// <param name="type">Type which arguments are built.</param>
        /// <param name="typeArguments">Arguments that are available for building given type.</param>
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
