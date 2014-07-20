using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;
using Analyzing;

namespace TypeSystem.Core
{
    /// <summary>
    /// Provider creating <see cref="GeneratorBase"/> objects which are stored by <see cref="MethodsCache"/>
    /// </summary>
    /// <param name="method">Method id of required method</param>
    /// <param name="definingAssembly">Assembly where method is defined</param>
    /// <returns>Provided <see cref="GeneratorBase"/></returns>
    delegate GeneratorBase GeneratorProvider(MethodID method, out AssemblyProvider definingAssembly);

    /// <summary>
    /// Provider providing <see cref="AssemblyProvider"/> which defines given <see cref="MethodID"/>
    /// </summary>
    /// <param name="method">Method which defining assembly is searched</param>
    /// <returns>Defining assembly if available, <c>null</c> otherwise</returns>
    delegate AssemblyProvider DefiningAssemblyProvider(MethodID method);

    /// <summary>
    /// Cache used for storing resolved methods. Cache supports naming invalidations.
    /// </summary>
    class MethodsCache
    {
        /// <summary>
        /// Storage for cached method generators
        /// </summary>
        private readonly Dictionary<MethodID, GeneratorBase> _cachedMethods = new Dictionary<MethodID, GeneratorBase>();

        /// <summary>
        /// Storage for cached method definers
        /// </summary>
        private readonly Dictionary<MethodID, AssemblyProvider> _cachedDefiners = new Dictionary<MethodID, AssemblyProvider>();

        /// <summary>
        /// Index used for quick methods invalidation
        /// </summary>
        private readonly MethodTrie _methodIdIndex = new MethodTrie();

        /// <summary>
        /// Event fired for every invalidated method
        /// </summary>
        internal event MethodInvalidationEvent MethodInvalidated;

        /// <summary>
        /// Get generator cached for given method. If it is not cached, provider is used and its result is cached
        /// </summary>
        /// <param name="method">Identifier of required method</param>
        /// <param name="provider">Provider that will be used in case of missing cached method generator</param>
        /// <returns>Generator for specified method</returns>
        internal GeneratorBase GetCachedGenerator(MethodID method, GeneratorProvider provider)
        {
            GeneratorBase cached;
            if (!_cachedMethods.TryGetValue(method, out cached))
            {
                //method is not cached yet - create it
                AssemblyProvider definingAssembly;
                cached = provider(method, out definingAssembly);
                if (cached != null)
                {
                    _methodIdIndex.Add(method);
                    _cachedMethods[method] = cached;
                    _cachedDefiners[method] = definingAssembly;
                }
            }

            return cached;
        }


        /// <summary>
        /// Get defining assembly cached for given method. If it is not cached, provider is used and its result is cached
        /// </summary>
        /// <param name="method">Identifier of method which assembly is searched</param>
        /// <param name="provider">Provider that will be used in case of missing cached defining assembly</param>
        /// <returns>Defining assembly of given method</returns>
        internal AssemblyProvider GetCachedDefiningAssembly(MethodID method, DefiningAssemblyProvider provider)
        {
            AssemblyProvider cached;
            if (!_cachedDefiners.TryGetValue(method, out cached))
            {
                cached = provider(method);
                if (cached != null)
                {
                    _cachedDefiners[method] = cached;
                }
            }

            return cached;
        }

        /// <summary>
        /// Invalidate methods that uses given prefix
        /// </summary>
        /// <param name="prefix">Prefix of invalidated methods</param>
        internal void Invalidate(string prefix)
        {
            if (prefix == null)
                return;

            var removed = _methodIdIndex.RemoveWithPrefix(prefix);
          
            foreach (var method in removed)
            {
                _cachedMethods.Remove(method);
                _cachedDefiners.Remove(method);
                if (MethodInvalidated != null)
                    MethodInvalidated(method);
            }
        }
    }

    class MethodTrie : WordTrie<string, MethodID>
    {
        public void Add(MethodID method)
        {
            var key = Naming.GetNonGenericPath(method);
            Add(key, method);
        }

        protected override IEnumerable<string> wordSpliter(string key)
        {
            var signature = PathInfo.GetNonGenericPath(key);
            return signature.Split(Naming.PathDelimiter);
        }
    }
}
