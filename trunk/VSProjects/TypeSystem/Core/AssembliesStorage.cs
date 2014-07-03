using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Core
{
    /// <summary>
    /// Storage for assemblies used by <see cref="AssembliesManager"/>.
    /// Distinguish between root and reference assemblies and provide automatical 
    /// reference assembly removing if needed.
    /// </summary>
    class AssembliesStorage
    {
        /// <summary>
        /// Manager that uses this storage
        /// </summary>
        private AssembliesManager _manager;

        /// <summary>
        /// In these assemblies are searched generators
        /// <remarks>May differ from method searcher providing assemblies - that are based on assembly references</remarks>
        /// </summary>
        readonly Dictionary<AssemblyProvider, TypeAssembly> _assemblies = new Dictionary<AssemblyProvider, TypeAssembly>();

        /// <summary>
        /// All loaded root assemblies
        /// </summary>
        readonly HashSet<AssemblyProvider> _rootAssemblies = new HashSet<AssemblyProvider>();

        /// <summary>
        /// Assembly providers indexed by their full paths
        /// </summary>
        readonly Dictionary<string, AssemblyProvider> _assemblyPathIndex = new Dictionary<string, AssemblyProvider>();

        /// <summary>
        /// Assembly providers indexed by their keys
        /// </summary>
        readonly Dictionary<object, AssemblyProvider> _assemblyKeyIndex = new Dictionary<object, AssemblyProvider>();

        /// <summary>
        /// All available assembly providers
        /// </summary>
        public IEnumerable<AssemblyProvider> Providers { get { return _assemblies.Keys; } }

        internal event AssemblyEvent OnRootRemove;

        internal event AssemblyEvent OnRootAdd;

        internal event AssemblyEvent OnRegistered;

        internal event AssemblyEvent OnUnRegistered;

        internal AssembliesStorage(AssembliesManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Add root assembly which composition points are displayed to the user. It 
        /// will be present within AppDomain until explicit unload.
        /// </summary>
        /// <param name="assembly">Added assembly</param>
        internal void AddRoot(AssemblyProvider assembly)
        {
            if (!_rootAssemblies.Add(assembly))
                //assembly is already contained
                return;


            registerAssembly(assembly);

            if (OnRootAdd != null)
                OnRootAdd(assembly);

        }

        /// <summary>
        /// Remove root assembly. If assembly is used as reference, it wont be removed completely.
        /// </summary>
        /// <param name="assembly">Removed assembly</param>
        internal void RemoveRoot(AssemblyProvider assembly)
        {
            if (!_rootAssemblies.Remove(assembly))
                //assembly is not root assembly
                return;

            //detect if root assembly is present somewhere as referenced assembly
            var isReferenced = false;
            foreach (var storedAssembly in _assemblies.Keys)
            {
                if (storedAssembly == assembly)
                    //self referenced assembly is not important
                    continue;

                if (storedAssembly.References.Contains(assembly.Key))
                {
                    isReferenced = true;
                    break;
                }
            }

            if (!isReferenced)
            {
                //we can remove it completely
                unregisterAssembly(assembly);
            }

            if (OnRootRemove != null)
                OnRootRemove(assembly);
        }

        /// <summary>
        /// Completely remove assembly - probably it has been invalidated
        /// </summary>
        /// <param name="assembly">Removed assembly</param>
        internal void Remove(AssemblyProvider assembly)
        {
            unregisterAssembly(assembly);
        }

        internal void AddReference(AssemblyProvider createdProvider)
        {
            registerAssembly(createdProvider);
        }

        internal AssemblyProvider FindProviderFromKey(object reference)
        {
            AssemblyProvider provider;
            _assemblyKeyIndex.TryGetValue(reference, out provider);

            return provider;
        }

        internal bool ContainsRealFile(string realFile)
        {
            return _assemblyPathIndex.ContainsKey(realFile);
        }

        internal TypeAssembly AccordingMappedFullpath(string assemblyPath)
        {
            //TODO performance improvement

            foreach (var assemblyPair in _assemblies)
            {
                if (assemblyPair.Key.FullPathMapping == assemblyPath)
                    return assemblyPair.Value;
            }

            return null;
        }

        internal TypeAssembly GetTypeAssembly(AssemblyProvider assemblyProvider)
        {
            if (assemblyProvider == null)
                return null;

            TypeAssembly typeAssembly;
            _assemblies.TryGetValue(assemblyProvider, out typeAssembly);

            return typeAssembly;
        }

        #region Private utilities

        /// <summary>
        /// Register given assembly if needed into storage
        /// </summary>
        /// <param name="assembly">Registered assembly</param>
        private void registerAssembly(AssemblyProvider assembly)
        {
            //assembly could be already registered
            if (!_assemblies.ContainsKey(assembly))
            {
                _assemblyPathIndex[assembly.FullPath] = assembly;
                _assemblyKeyIndex[assembly.Key] = assembly;

                var typeAssembly = new TypeAssembly(_manager, assembly);
                _assemblies.Add(assembly, typeAssembly);

                if (OnRegistered != null)
                    OnRegistered(assembly);
            }
        }

        /// <summary>
        /// Unregister given assembly if needed from storage
        /// </summary>
        /// <param name="assembly">Unregistered assembly</param>
        private void unregisterAssembly(AssemblyProvider assembly)
        {
            if (_assemblies.Remove(assembly))
            {
                _assemblyPathIndex.Remove(assembly.FullPath);
                _assemblyKeyIndex.Remove(assembly.Key);

                if (OnUnRegistered != null)
                    OnUnRegistered(assembly);
            }
        }

        #endregion


        internal bool IsRequired(object p)
        {
            throw new NotImplementedException();
        }
    }
}
