using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.TypeParsing;

namespace TypeSystem
{
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
        /// Type arguments of current type descriptor
        /// </summary>
        private readonly Dictionary<string, TypeDescriptor> _typeArguments;

        /// <summary>
        /// Returns enumeration of arguments ordered as in type
        /// </summary>
        public IEnumerable<string> Arguments
        {
            get
            {
                //TODO make sure that correct ordering is returned
                return _typeArguments.Keys;
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
            //TODO parse given name
            return new TypeDescriptor(typeName);
        }

        /// <summary>
        /// Create type descriptor from given type
        /// </summary>
        /// <param name="type">Type from which descriptor will be created</param>
        /// <returns>Created type descriptor</returns>
        public static TypeDescriptor Create(Type type)
        {
            var builder = new TypeDescriptorBuilder();
            buildType(type, builder);

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
        private static void buildType(Type type, TypeDescriptorBuilder builder)
        {
            if (type.IsArray)
            {
                buildArray(type, builder);
            }
            else if (type.IsGenericParameter)
            {
                builder.SetParameter(type.Name);
                return;
            }
            else
            {
                var genericArgs = new Queue<Type>(type.GetGenericArguments());
                buildTypeChain(type, builder, genericArgs);
            }
        }

        /// <summary>
        /// Director for building given arrayType with builder
        /// </summary>
        /// <param name="arrayType">Builded type</param>
        /// <param name="builder">Builder used by director</param>
        private static void buildArray(Type arrayType, TypeDescriptorBuilder builder)
        {
            builder.Append("Array");
            builder.Push();
            buildType(arrayType.GetElementType(), builder);
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
        private static void buildArguments(Type type, TypeDescriptorBuilder builder, Queue<Type> typeArguments)
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
                buildType(substitution, builder);
                builder.Pop();
            }
        }

        /// <summary>
        /// Director for building given type chain (DeclaringType chain) with builder
        /// </summary>
        /// <param name="type">Type available for builded subchain</param>
        /// <param name="builder">Builder used by director</param>
        /// <param name="typeArguments">Builded type arguments</param>
        private static void buildTypeChain(Type type, TypeDescriptorBuilder builder, Queue<Type> typeArguments)
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
                buildTypeChain(declaringType, builder, availableArguments);
                builder.Push();
            }

            buildName(type, builder);
            buildArguments(type, builder, availableArguments);

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
