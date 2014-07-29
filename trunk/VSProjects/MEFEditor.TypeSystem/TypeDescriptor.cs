using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem.TypeParsing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Resolver for type parameters. Is used for translating type parameters
    /// into TypeDescriptor.
    /// </summary>
    /// <param name="type">Translated type.</param>
    /// <returns>Resolved TypeDescriptor.</returns>
    public delegate TypeDescriptor ParameterResolver(Type type);

    /// <summary>
    /// Resolver for substitutions of generic paths
    /// </summary>
    /// <param name="genericParameter">Parameter to be substituted</param>
    /// <returns>Substitution</returns>
    public delegate string SubstitutionResolver(string genericParameter);

    /// <summary>
    /// <see cref="InstanceInfo" /> implementation used by TypeSystem. It
    /// provides way how to describe types independently of the type itself.
    /// </summary>
    public class TypeDescriptor : InstanceInfo
    {
        /// <summary>
        /// Regex that is used for replacing type parameters in fullname
        /// <remarks>Note that dots are included if available - this prevents replacing namespace parts</remarks>.
        /// </summary>
        private static readonly Regex _typeReplacement = new Regex(@"[@a-zA-Z01-9.]+[,>]", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Generic information used for arrays.
        /// </summary>
        public static readonly TypeDescriptor ArrayInfo = TypeDescriptor.Create("Array<@0,@1>");

        /// <summary>
        /// Signature of Array{}.
        /// </summary>
        public static readonly string ArraySignature = PathInfo.GetSignature(ArrayInfo);

        /// <summary>
        /// Type descriptor for object.
        /// </summary>
        public static readonly TypeDescriptor ObjectInfo = TypeDescriptor.Create<object>();

        /// <summary>
        /// Type descriptor for instance.
        /// </summary>
        public static readonly TypeDescriptor InstanceInfo = TypeDescriptor.Create<Instance>();

        /// <summary>
        /// Type descriptor for IEnumerable{}.
        /// </summary>
        public static readonly TypeDescriptor IEnumerableGenericInfo = TypeDescriptor.Create(typeof(IEnumerable<>));

        /// <summary>
        /// Signature of IEnumerable{}.
        /// </summary>
        public static readonly string IEnumerableSignature = PathInfo.GetSignature(IEnumerableGenericInfo);

        /// <summary>
        /// Type descriptor for ICollection{}.
        /// </summary>
        public static readonly TypeDescriptor ICollectionGenericInfo = TypeDescriptor.Create(typeof(ICollection<>));

        /// <summary>
        /// Signature of ICollection{}.
        /// </summary>
        public static readonly string ICollectionSignature = PathInfo.GetSignature(ICollectionGenericInfo);

        /// <summary>
        /// Shortcut for Void type descriptor.
        /// </summary>
        public static readonly TypeDescriptor Void = Create(typeof(void));

        /// <summary>
        /// Shortcut for empty argument arrays.
        /// </summary>
        public static readonly TypeDescriptor[] NoDescriptors = new TypeDescriptor[0];

        /// <summary>
        /// Type arguments of current type descriptor.
        /// </summary>
        private readonly Dictionary<string, TypeDescriptor> _typeArguments;

        /// <summary>
        /// Returns enumeration of arguments ordered as in type.
        /// </summary>
        /// <value>The arguments.</value>
        public IEnumerable<TypeDescriptor> Arguments
        {
            get
            {
                //TODO make sure that correct resolver is returned
                return _typeArguments.Values;
            }
        }

        /// <summary>
        /// Determine that descriptor belongs to type parameter.
        /// </summary>
        /// <value><c>true</c> if this instance is parameter; otherwise, <c>false</c>.</value>
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
        /// <value><c>true</c> if this instance has parameters; otherwise, <c>false</c>.</value>
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
        /// Gets the hint for default <see cref="Instance" />identifier.
        /// </summary>
        /// <value>The default identifier hint.</value>
        /// <inheritdoc />
        public override string DefaultIdHint
        {
            get
            {
                var shortName = PathInfo.GetSignature(this).Split('.').Last();
                shortName = char.ToLowerInvariant(shortName[0]) + shortName.Substring(1);
                return shortName;
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
        /// Make generic version of type descriptor using given substitutions.
        /// </summary>
        /// <param name="substitutions">Substitutions for making generic version.</param>
        /// <returns>Created generic version.</returns>
        public TypeDescriptor MakeGeneric(Dictionary<string, string> substitutions)
        {
            var name = TypeName;

            name = TranslatePath(name, substitutions, true);

            return TypeDescriptor.Create(name);
        }

        /// <summary>
        /// Translate given path according to given substitutions.
        /// </summary>
        /// <param name="path">Translated path.</param>
        /// <param name="resolver">Resolver used for substitutions.</param>
        /// <param name="replaceSelf">if set to <c>true</c> whole path substitution will be processed.</param>
        /// <returns>Translated path.</returns>
        public static string TranslatePath(string path, SubstitutionResolver resolver, bool replaceSelf = false)
        {
            var initialPath = replaceSelf ? resolver(path) : path;

            var replaced = _typeReplacement.Replace(initialPath, (m) =>
            {
                var toReplace = m.ToString();
                var matched = toReplace.Substring(0, m.Length - 1);
                return resolver(matched) + toReplace[m.Length - 1];
            });

            return replaced;
        }

        /// <summary>
        /// Translate given path according to given substitutions.
        /// </summary>
        /// <param name="path">Translated path.</param>
        /// <param name="substitutions">Substitutions used for translation.</param>
        /// <param name="replaceSelf">if set to <c>true</c> whole path substitution will be processed.</param>
        /// <returns>Translated path.</returns>
        public static string TranslatePath(string path, Dictionary<string, string> substitutions, bool replaceSelf = false)
        {
            return TranslatePath(path, (pathParameter) =>
            {
                string result;
                if (
                    substitutions.TryGetValue(pathParameter, out result) ||
                    substitutions.TryGetValue('@' + pathParameter, out result)
                    )
                    //substitution is processed
                    return result;

                //no replacement
                return pathParameter;
            }, replaceSelf);
        }

        /// <summary>
        /// Creates type descriptor from given name.
        /// </summary>
        /// <param name="typeName">Type name that will be parsed.</param>
        /// <returns>Created type descriptor.</returns>
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
        /// Create type descriptor from given type.
        /// </summary>
        /// <param name="type">Type from which descriptor will be created.</param>
        /// <param name="resolver">The generic parameter resolver.</param>
        /// <returns>Created type descriptor.</returns>
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
        /// Create type descriptor from given type.
        /// </summary>
        /// <typeparam name="T">Type from which descriptor will be created.</typeparam>
        /// <returns>Created type descriptor.</returns>
        public static TypeDescriptor Create<T>()
        {
            return TypeDescriptor.Create(typeof(T));
        }

        #endregion
    }
}
