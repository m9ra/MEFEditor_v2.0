using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.TypeParsing;

namespace TypeSystem
{

    public delegate TypeDescriptor ParameterResolver(Type type);

    /// <summary>
    /// InstanceInfo implementation used by TypeSystem
    /// </summary>
    public class TypeDescriptor : InstanceInfo
    {
        /// <summary>
        /// Shortcut for Void type descriptor
        /// </summary>
        public static readonly TypeDescriptor Void = Create(typeof(void));

        /// <summary>
        /// Shortcut for empty argument arrays
        /// </summary>
        public static readonly TypeDescriptor[] NoDescriptors = new TypeDescriptor[0];


        /// <summary>
        /// Type arguments of current type descriptor
        /// </summary>
        private readonly Dictionary<string, TypeDescriptor> _typeArguments;

        /// <summary>
        /// Returns enumeration of arguments ordered as in type
        /// </summary>
        public IEnumerable<TypeDescriptor> Arguments
        {
            get
            {
                //TODO make sure that correct resolver is returned
                return _typeArguments.Values;
            }
        }


        public bool IsParameter
        {
            get
            {
                //TODO refactor
                return TypeName.StartsWith("@");
            }
        }

        public bool HasParameters
        {
            get
            {
                foreach (var typeArg in _typeArguments)
                {
                    if (typeArg.Value == null || typeArg.Value.IsParameter)
                        return true;
                }

                return false;
            }
        }

        internal TypeDescriptor(string typeName, Dictionary<string, TypeDescriptor> typeArguments = null)
            : base(typeName)
        {
            if (typeArguments == null)
            {
                _typeArguments = new Dictionary<string, TypeDescriptor>();
            }
            else
            {
                //defensive copy
                _typeArguments = new Dictionary<string, TypeDescriptor>(typeArguments);
            }
        }

        #region Factory methods for type descriptor creation

        /// <summary>
        /// Creates type descriptor from given name
        /// </summary>
        /// <param name="typeName">Type name that will be parsed</param>
        /// <returns>Created type descriptor</returns>
        public static TypeDescriptor Create(string typeName)
        {
            //TODO improve type parsing
            var path = new PathInfo(typeName);
            var args = path.GenericArgs.ToArray();

            var typeArguments = new Dictionary<string, TypeDescriptor>();
            for (int i = 0; i < path.GenericArgs.Count; ++i)
            {
                var argKey = "@" + i;

                path.GenericArgs[i] = argKey;
                var typeArgument = TypeDescriptor.Create(args[i]);
                typeArguments.Add(argKey, typeArgument);
            }

            return new TypeDescriptor(typeName, typeArguments);
        }

        public static TypeDescriptor GetParameter(int parameterIndex)
        {
            //TODO refactor!!
            return new TypeDescriptor("@" + parameterIndex);
        }

        /// <summary>
        /// Create type descriptor from given type
        /// </summary>
        /// <param name="type">Type from which descriptor will be created</param>
        /// <returns>Created type descriptor</returns>
        public static TypeDescriptor Create(Type type, ParameterResolver resolver = null)
        {
            var builder = new TypeDescriptorBuilder();

            if (resolver == null)
            {
                //create default routine for parameters resolver
                var parameters = new Dictionary<Type, int>();
                resolver = (t) =>
                {
                    int number;
                    if (!parameters.TryGetValue(t, out number))
                    {
                        number = parameters.Count;
                        parameters.Add(t, number);
                    }

                    return TypeDescriptor.GetParameter(number);
                };
            }

            buildType(type, builder, resolver);
            return builder.BuildDescriptor();
        }

        /// <summary>
        /// Create type descriptor from given type
        /// </summary>
        /// <typeparam name="T">Type from which descriptor will be created</typeparam>
        /// <returns>Created type descriptor</returns>
        public static TypeDescriptor Create<T>()
        {
            return TypeDescriptor.Create(typeof(T));
        }

        #endregion

        /// <summary>
        /// Director for building given type with builder
        /// </summary>
        /// <param name="type">Builded type</param>
        /// <param name="builder">Builder used by director</param>
        private static void buildType(Type type, TypeDescriptorBuilder builder, ParameterResolver resolver)
        {
            if (type.IsArray)
            {
                buildArray(type, builder, resolver);
            }
            else if (type.IsGenericParameter)
            {
                var parameter = resolver(type);
                builder.SetDescriptor(parameter);
                return;
            }
            else
            {
                var genericArgs = new Queue<Type>(type.GetGenericArguments());
                buildTypeChain(type, builder, genericArgs, resolver);
            }
        }

        /// <summary>
        /// Director for building given arrayType with builder
        /// </summary>
        /// <param name="arrayType">Builded type</param>
        /// <param name="builder">Builder used by director</param>
        private static void buildArray(Type arrayType, TypeDescriptorBuilder builder, ParameterResolver resolver)
        {
            builder.Append("Array");
            builder.Push();
            buildType(arrayType.GetElementType(), builder, resolver);
            builder.Pop();

            //TODO refactor dimension argument handling
            builder.InsertArgument("1");
        }

        /// <summary>
        /// Director for building given type arguments with builder
        /// </summary>
        /// <param name="type">Type which arguments are builded</param>
        /// <param name="builder">Builder used by director</param>
        /// <param name="typeArguments">Builded type arguments</param>
        private static void buildArguments(Type type, TypeDescriptorBuilder builder, Queue<Type> typeArguments, ParameterResolver resolver)
        {
            var typeParams = type.GetGenericArguments();

            for (int i = 0; i < typeParams.Length; ++i)
            {
                if (typeArguments.Count == 0)
                {
                    //all arguments has already been substituted
                    return;
                }

                //take only fist params  that will be substituted
                var substitution = typeArguments.Dequeue();

                builder.Push();
                buildType(substitution, builder, resolver);
                builder.Pop();
            }
        }

        /// <summary>
        /// Director for building given type chain (DeclaringType chain) with builder
        /// </summary>
        /// <param name="type">Type available for builded subchain</param>
        /// <param name="builder">Builder used by director</param>
        /// <param name="typeArguments">Builded type arguments</param>
        private static void buildTypeChain(Type type, TypeDescriptorBuilder builder, Queue<Type> typeArguments, ParameterResolver resolver)
        {
            var declaringType = type.DeclaringType;
            var hasConnectedType = declaringType != null;

            //take as much arguments, as connected types needs + (mine types)
            var parameters = type.GetGenericArguments();
            var availableArguments = new Queue<Type>();
            for (int i = 0; i < parameters.Length; ++i)
            {
                availableArguments.Enqueue(typeArguments.Dequeue());
            }

            if (hasConnectedType)
            {
                buildTypeChain(declaringType, builder, availableArguments, resolver);
                builder.Push();
            }

            buildName(type, builder);
            buildArguments(type, builder, availableArguments, resolver);

            if (hasConnectedType)
            {
                builder.ConnectPop();
            }
        }

        /// <summary>
        /// Director for building given type name with builder
        /// </summary>
        /// <param name="type">Type which name is builded</param>
        /// <param name="builder">Builder used by director</param>
        private static void buildName(Type type, TypeDescriptorBuilder builder)
        {
            var name = type.Name;
            var endName = name.IndexOf('`');
            if (endName > 0)
                name = name.Substring(0, endName);

            builder.Append(type.Namespace);
            builder.Append(name);
        }


    }
}
