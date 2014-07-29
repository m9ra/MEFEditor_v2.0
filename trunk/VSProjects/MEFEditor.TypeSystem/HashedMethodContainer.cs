using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using Utilities;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Utility class for handling method definitions in sequential structured
    /// assemblies. It uses hashing for fast method searching. It is not appropriate for
    /// tree structured assemblies.
    /// </summary>
    public class HashedMethodContainer
    {
        /// <summary>
        /// The method items indexed by their path signatures.
        /// </summary>
        readonly private MultiDictionary<string, MethodItem> _methodPaths = new MultiDictionary<string, MethodItem>();

        /// <summary>
        /// The method items indexed by their identifiers.
        /// </summary>
        readonly private Dictionary<MethodID, MethodItem> _methodIds = new Dictionary<MethodID, MethodItem>();

        /// <summary>
        /// The explicit table of relations between method and implemented types.
        /// </summary>
        readonly private Dictionary<Tuple<TypeDescriptor, MethodID>, MethodID> _explicitImplementations = new Dictionary<Tuple<TypeDescriptor, MethodID>, MethodID>();

        /// <summary>
        /// The table of generic implementation relations.
        /// </summary>
        readonly private Dictionary<Tuple<string, string>, MethodItem> _genericImplementations = new Dictionary<Tuple<string, string>, MethodItem>();

        /// <summary>
        /// Adds the item to current container.
        /// </summary>
        /// <param name="item">The added item.</param>
        /// <param name="implementedTypes">The types that are implemented by method item.</param>
        public void AddItem(MethodItem item, IEnumerable<InstanceInfo> implementedTypes = null)
        {
            if (implementedTypes == null)
                implementedTypes = new InstanceInfo[0];

            registerImplementedMethods(item, implementedTypes);

            var itemInfo = item.Info;
            var itemPath = itemInfo.Path;

            if (!_methodIds.ContainsKey(itemInfo.MethodID))
                _methodIds.Add(itemInfo.MethodID, item);

            _methodPaths.Add(itemPath.ShortSignature, item);
        }

        /// <summary>
        /// Gets the method items.
        /// </summary>
        /// <value>Stored method items.</value>
        public KeyValuePair<MethodID, MethodItem>[] MethodItems
        {
            get
            {
                return _methodIds.ToArray();
            }
        }

        /// <summary>
        /// Registers implemented types of given method.
        /// </summary>
        /// <param name="item">The item which implementing types are registered.</param>
        /// <param name="implementedTypes">The implemented types.</param>
        private void registerImplementedMethods(MethodItem item, IEnumerable<InstanceInfo> implementedTypes)
        {
            foreach (var implementedType in implementedTypes)
            {
                var implementingType = item.Info.DeclaringType;
                var implementingMethodID = item.Info.MethodID;

                if (item.Info.HasGenericParameters)
                {
                    var implementingPath = new PathInfo(implementingType.TypeName);
                    var genericImplementedMethod = Naming.ChangeDeclaringType(implementedType.TypeName, implementingMethodID, true);
                    var implementedMethodPath = Naming.GetMethodPath(genericImplementedMethod);

                    var genericImplementsEntry = Tuple.Create(implementingPath.Signature, implementedMethodPath.Signature);
                    _genericImplementations.Add(genericImplementsEntry, item);
                }
                else
                {
                    var implementedMethod = Naming.ChangeDeclaringType(implementedType.TypeName, implementingMethodID, true);
                    var implementsEntry = Tuple.Create(implementingType, implementedMethod);
                    _explicitImplementations.Add(implementsEntry, implementingMethodID);
                }
            }
        }

        /// <summary>
        /// Method that is used for searching method info according to path - method info is instantiated
        /// according to generic.
        /// </summary>
        /// <param name="path">Path of searched methods.</param>
        /// <returns>Found methods.</returns>
        public IEnumerable<TypeMethodInfo> AccordingPath(PathInfo path)
        {
            var overloads = from overload in accordingPath(path) select overload.Info;
            return overloads;
        }

        /// <summary>
        /// Method that is used for searching method info of given type path - method info is NOT instantiated
        /// according to generic.
        /// </summary>
        /// <param name="typePath">Path of type which methods are searched.</param>
        /// <returns>Found methods.</returns>
        internal IEnumerable<TypeMethodInfo> AccordingType(PathInfo typePath)
        {
            var signature = typePath.ShortSignature;
            foreach (var key in _methodPaths.Keys)
            {
                if (key.Length <= signature.Length)
                    continue;

                var isSearchedType = key.StartsWith(signature) && !key.Substring(signature.Length + 1).Contains('.');
                if (isSearchedType)
                {
                    foreach (var method in _methodPaths.Get(key))
                        yield return method.Info;
                }
            }
        }

        /// <summary>
        /// Search method according to given method identifier.
        /// </summary>
        /// <param name="method">The searched method identifier.</param>
        /// <returns>Generator of method if available, <c>null</c> otherwise.</returns>
        /// <exception cref="System.NotSupportedException">Cannot get method with generic parameters</exception>
        public GeneratorBase AccordingId(MethodID method)
        {
            MethodItem item;
            if (_methodIds.TryGetValue(method, out item))
            {
                if (item.Info.HasGenericParameters)
                {
                    throw new NotSupportedException("Cannot get method with generic parameters " + method);
                }

                return item.Generator;
            }

            //method hasn't been found
            return null;
        }

        /// <summary>
        /// Removes the specified method from container.
        /// </summary>
        /// <param name="method">The identifier of removed method.</param>
        public void RemoveItem(MethodID method)
        {
            MethodItem item;
            if (!_methodIds.TryGetValue(method, out item))
                //nothing to remove
                return;

            _methodIds.Remove(method);
            _methodPaths.Remove(Naming.GetMethodPath(method).ShortSignature, item);
        }

        /// <summary>
        /// Search generic method according to given method identifier.
        /// </summary>
        /// <param name="method">The searched method identifier.</param>
        /// <param name="searchPath">Path where generic arguments can be found.</param>
        /// <returns>Generator of method if available, <c>null</c> otherwise.</returns>
        public GeneratorBase AccordingGenericId(MethodID method, PathInfo searchPath)
        {
            var overloads = accordingPath(searchPath);
            foreach (var overload in overloads)
            {
                if (overload.Info.MethodID.Equals(method))
                {
                    return overload.Generator;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the implementation of virtual method.
        /// </summary>
        /// <param name="method">The method which implementation is searched.</param>
        /// <param name="dynamicInfo">The call time type information.</param>
        /// <returns>Implementing method identifier if found, <c>null</c> otherwise.</returns>
        public MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo)
        {
            var implementationEntry = Tuple.Create(dynamicInfo, method);

            MethodID implementation;
            _explicitImplementations.TryGetValue(implementationEntry, out implementation);

            return implementation;
        }

        /// <summary>
        /// Gets the implementation of virtual method.
        /// </summary>
        /// <param name="method">The method which implementation is searched.</param>
        /// <param name="methodSearchPath">Path of searched method with generic arguments.</param>
        /// <param name="implementingTypePath">Path of implementing type.</param>
        /// <returns>Implementing method identifier if found, <c>null</c> otherwise.</returns>
        public MethodID GetGenericImplementation(MethodID method, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            var implementationEntry = Tuple.Create(implementingTypePath.Signature, methodSearchPath.Signature);

            MethodItem implementation;
            if (!_genericImplementations.TryGetValue(implementationEntry, out implementation))
                //implementation not found
                return null;

            var implementingMethod = Naming.ChangeDeclaringType(implementingTypePath.Name, method, false);
            var implementingMethodPath = Naming.GetMethodPath(implementingMethod);
            var genericImplementation = implementation.Make(implementingMethodPath);

            return genericImplementation.Info.MethodID;
        }

        /// <summary>
        /// Method that is used for searching method info according to path - method info is instantiated
        /// according to generic.
        /// </summary>
        /// <param name="path">Path of searched methods.</param>
        /// <returns>Found methods.</returns>
        private IEnumerable<MethodItem> accordingPath(PathInfo path)
        {
            var methods = _methodPaths.Get(path.ShortSignature);
            foreach (var methodItem in methods)
            {
                if (methodItem.Info.HasGenericParameters)
                {
                    yield return methodItem.Make(path);
                }
                else
                {
                    yield return methodItem;
                }
            }
        }
    }
}
