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
    /// Resolver for type parameters. Is used for translating type parameters
    /// into TypeDescriptor.
    /// </summary>
    /// <param name="type">Translated type.</param>
    /// <returns>Resolved TypeDescriptor.</returns>
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

        /// <summary>
        /// Determine that descriptor belongs to type parameter
        /// </summary>
        public bool IsParameter
        {
            get
            {
                //TODO refactor
                return TypeName.StartsWith("@");
            }
        }

        /// <summary>
        /// Determine that current descriptor has argument that is
        /// marked as parameter. Is searched recursively within parameters.
        /// </summary>
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

        /// <summary>
        /// Create type descriptor of given name and arguments.
        /// </summary>
        /// <param name="typeName">Name of described type.</param>
        /// <param name="typeArguments">Type arguments for described type.</param>
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

        /// <summary>
        /// Get descriptor marked as parameter with given index.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter.</param>
        /// <returns>Parameter descritor.</returns>
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

            var adapter = new TypeAdapter(type);
            var result = TypeHierarchyDirector.BuildDescriptor(adapter, (a) => resolver((a as TypeAdapterBase<Type>).AdaptedType));

            return result;
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
    }
}
