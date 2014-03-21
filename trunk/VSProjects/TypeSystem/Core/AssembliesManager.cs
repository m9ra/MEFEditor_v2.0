using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Utilities;
using Analyzing;
using TypeSystem.Runtime;


namespace TypeSystem.Core
{
    /// <summary>
    /// Representation of AppDomain, that handle loading/unloading, type resolving and components. Is topmost manager
    /// for TypeSystem services
    /// </summary>
    class AssembliesManager
    {
        /// <summary>
        /// Assemblies that are currently loaded
        /// </summary>
        private readonly AssembliesStorage _assemblies;

        /// <summary>
        /// Loader that is used for creating assemblies
        /// </summary>
        private readonly AssemblyLoader _loader;

        /// <summary>
        /// Components indexed by their defining assemblies
        /// </summary>
        private readonly MultiDictionary<AssemblyProvider, ComponentInfo> _assemblyComponents = new MultiDictionary<AssemblyProvider, ComponentInfo>();

        /// <summary>
        /// Components indexed by defining types
        /// </summary>
        private readonly Dictionary<InstanceInfo, ComponentInfo> _components = new Dictionary<InstanceInfo, ComponentInfo>();

        /// <summary>
        /// Settings available fur current AppDomain
        /// </summary>
        internal readonly MachineSettings Settings;

        /// <summary>
        /// Event fired whenever new component is added
        /// </summary>
        internal event ComponentEvent ComponentAdded;

        /// <summary>
        /// Event fired whenever component is removed
        /// </summary>
        internal event ComponentEvent ComponentRemoved;

        /// <summary>
        /// Event fired whenever new assembly is added into AppDomain
        /// </summary>
        internal event AssemblyEvent AssemblyAdded;

        /// <summary>
        /// Event fired whenever assembly is removed from AppDomain
        /// </summary>
        internal event AssemblyEvent AssemblyRemoved;

        /// <summary>
        /// Enumeration of all available components
        /// </summary>
        internal IEnumerable<ComponentInfo> Components { get { return _components.Values; } }

        /// <summary>
        /// All loaded assemblies
        /// </summary>
        public IEnumerable<AssemblyProvider> Assemblies { get { return _assemblies.Providers; } }

        /// <summary>
        /// Runtime used by current AppDomain
        /// </summary>
        internal RuntimeAssembly Runtime { get { return Settings.Runtime; } }

        internal AssembliesManager(AssemblyLoader loader, MachineSettings settings)
        {
            Settings = settings;

            _loader = loader;

            _assemblies = new AssembliesStorage(this);
            _assemblies.OnRootAdd += _onRootAssemblyAdd;
            _assemblies.OnRootRemove += _onAssemblyRemove;
            _assemblies.OnRegistered += _onRootRegistered;

            //runtime assembly has to be present
            _assemblies.AddRoot(settings.Runtime);
        }

        #region Workflow definitions

        /// <summary>
        /// Tries to recover assembly that has been invalidated
        /// </summary>
        /// <param name="assemblyKey">Assembly key describes recovered assembly</param>
        private void tryRecoverAssembly(object assemblyKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reload assemblies that are affected by given assembly key
        /// </summary>
        /// <param name="key">Key that is affecting assemblies</param>
        private void reloadAffectedAssemblies(object key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Force reloading of given assembly (probably because of invalidation of some its references)
        /// </summary>
        /// <param name="assembly">Reloaded assembly</param>
        private void reloadAssembly(AssemblyProvider assembly)
        {
            invalidateDefinedComponents(assembly);
            loadComponents(assembly);
        }

        /// <summary>
        /// Force invalidation of all components that are defined within given assembly
        /// </summary>
        /// <param name="assembly">Assembly which components will be invalidated</param>
        private void invalidateDefinedComponents(AssemblyProvider assembly)
        {
            var components = GetComponents(assembly);
            foreach (var component in components)
            {
                invalidateComponent(assembly, component);
            }
        }

        /// <summary>
        /// Force invalidation of given component
        /// </summary>
        /// <param name="definingAssembly">Assembly where component is defined</param>
        /// <param name="component">Invalidated component</param>
        private void invalidateComponent(AssemblyProvider definingAssembly, ComponentInfo component)
        {
            //behaves same as real removing from assembly provider
            _onComponentRemoved(definingAssembly, component);
        }

        /// <summary>
        /// Force loading components from given assembly
        /// </summary>
        /// <param name="assembly">Assembly which components will be loaded</param>
        private void loadComponents(AssemblyProvider assembly)
        {
            assembly.LoadComponents();
        }

        /// <summary>
        /// Completely remove assembly - probably it has been invalidated
        /// </summary>
        /// <param name="assembly">Removed assembly</param>
        private void removeAssembly(AssemblyProvider assembly)
        {
            //this will cause unregister events on assembly
            _assemblies.Remove(assembly);
        }

        #endregion

        #region Internal methods exposed for AssemblyLoader


        internal GeneratorBase StaticResolve(MethodID method)
        {
            var result = tryStaticResolve(method);

            if (result == null)
                throw new NotSupportedException("Invalid method: " + method);

            return result;
        }

        internal MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            //resolving of .NET objects depends only on called object type
            var calledObjectType = dynamicArgumentInfo[0] as TypeDescriptor;
            var methodImplementation = tryDynamicResolve(calledObjectType, method);

            if (methodImplementation == null)
                throw new NotSupportedException("Doesn't have " + calledObjectType + " implementation for: " + method);

            return methodImplementation;
        }

        #endregion

        #region Internal methods exposed for TypeServices

        #region Reference API

        /// <summary>
        /// Given reference has been removed from given <see cref="AssemblyProvider"/>. Removing assembly provider
        /// does not need this reference, however other providers may it still referenced
        /// </summary>
        /// <param name="assembly">Assembly which references changed</param>
        /// <param name="reference">Removed reference</param>
        internal void ReportReferenceRemoved(AssemblyProvider assembly, object reference)
        {
            _onReferenceRemoved(assembly, reference);
        }

        /// <summary>
        /// Given reference has been added into given <see cref="AssemblyProvider"/>. If the
        /// referenced assembly doesnot exists it has to be loaded
        /// </summary>
        /// <param name="assembly">Assembly which references changed</param>
        /// <param name="reference">Reference that has been added into assembly</param>
        internal void ReportReferenceAdded(AssemblyProvider assembly, object reference)
        {
            _onReferenceAdded(assembly, reference);
        }

        #endregion

        #region Type inspection API

        /// <summary>
        /// Get concrete implementation of abstract (virtual,interface,..) method on given type
        /// </summary>
        /// <param name="type">Type where concrete implementation is searched</param>
        /// <param name="abstractMethod">Abstract method which implementation is searched</param>
        /// <returns>Concreate implementation if available, <c>null</c> otherwise</returns>
        internal MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return tryDynamicResolve(type, abstractMethod);
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        internal MethodSearcher CreateSearcher(ReferencedAssemblies references)
        {
            return new MethodSearcher(resolveKeys(references));
        }

        /// <summary>
        /// Determine that assignedType can be assigned into variable with targetTypeName without any conversion calls (implicit nor explicit)
        /// Only tests inheritance
        /// </summary>
        /// <param name="targetTypeName">Name of target variable type</param>
        /// <param name="assignedTypeName">Name of assigned type</param>
        /// <returns>True if assigned type is assignable, false otherwise</returns>
        internal bool IsAssignable(string targetTypeName, string assignedTypeName)
        {
            if (targetTypeName == assignedTypeName)
                return true;

            var chain = getChain(assignedTypeName);
            return chain.HasSubChain(targetTypeName);
        }

        /// <summary>
        /// Create inheritance chain for given type and subChains
        /// <remarks>This is used by <see cref="AssemblyProvider"/> to create information about inheritance</remarks>
        /// </summary>
        /// <param name="type">Type which inheritance chain is created</param>
        /// <param name="subChains"><see cref="InheritanceChains"/> of sub types</param>
        /// <returns>Created chain</returns>
        internal InheritanceChain CreateChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            //TODO caching

            return new InheritanceChain(type, subChains);
        }

        /// <summary>
        /// Get inheritance chain for given type
        /// </summary>
        /// <param name="type">Type which inheritance chain is desired</param>
        /// <returns>Founded inheritance chain if available, <c>null</c> otherwise</returns>
        internal InheritanceChain GetChain(TypeDescriptor type)
        {
            return getChain(type.TypeName);
        }

        #endregion

        #region Component API

        /// <summary>
        /// Get <see cref="ComponentInfo"/> defined for given type.
        /// </summary>
        /// <param name="type">Type which component info is needed</param>
        /// <returns><see cref="ComponentInfo"/> defined for type if available, <c>false</c> otherwise</returns>
        internal ComponentInfo GetComponentInfo(InstanceInfo type)
        {
            ComponentInfo result;
            _components.TryGetValue(type, out result);
            return result;
        }

        /// <summary>
        /// Get components defined within given assembly
        /// </summary>
        /// <param name="assembly">Assembly where components are searched</param>
        /// <returns>Components defined within assembly</returns>
        internal IEnumerable<ComponentInfo> GetComponents(AssemblyProvider assembly)
        {
            return _assemblyComponents.Get(assembly);
        }

        #endregion

        #region Assembly API

        /// <summary>
        /// Get files that are present in given directory by taking assemblies mapping into consideration
        /// </summary>
        /// <param name="directoryFullPath">Fullpath of directory which files will be retrieved</param>
        /// <returns>Files that are present in directory according to virtual mapping</returns>
        internal IEnumerable<string> GetFiles(string directoryFullPath)
        {
            var realFiles = Directory.GetFiles(directoryFullPath);

            //get real files filtered by mapped assemblies
            foreach (var realFile in realFiles)
            {
                if (_assemblies.ContainsRealFile(realFile))
                    //assemblies are added according to their mapping
                    continue;

                yield return realFile;
            }

            //get files that are added by virtual mapping
            foreach (var assembly in _assemblies.Providers)
            {
                if (!assembly.FullPathMapping.StartsWith(directoryFullPath))
                    //directory has to match begining of path
                    continue;

                var mappedDirectory = Path.GetDirectoryName(assembly.FullPathMapping);
                if (mappedDirectory == directoryFullPath)
                    //virtual mapping match
                    yield return assembly.FullPathMapping;
            }
        }

        /// <summary>
        /// Load assembly for purposes of interpretation analysis. Assembly is automatically cached between multiple runs.
        /// Mapping of assemblies is take into consideration
        /// </summary>
        /// <param name="assemblyKey">Key of loaded assembly</param>
        /// <returns>Loaded assembly if available, <c>null</c> otherwise</returns>
        internal TypeAssembly LoadReferenceAssembly(object assemblyKey)
        {
            var assembly = findLoadedAssembly(assemblyKey);
            if (assembly != null)
                //assembly has been created in the past
                return assembly;

            var createdProvider = createAssembly(assemblyKey);
            if (createdProvider == null)
                //assembly is not available
                return null;

            //register created assembly
            _assemblies.AddReference(createdProvider);

            return _assemblies.GetTypeAssembly(createdProvider);
        }

        /// <summary>
        /// Load root assembly into AppDomain
        /// </summary>
        /// <param name="loadedAssembly">Asemlby that is loaded</param>
        internal void LoadRoot(AssemblyProvider loadedAssembly)
        {
            //adding will fire appropriate handlers
            _assemblies.AddRoot(loadedAssembly);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly where method is defined</returns>
        internal TypeAssembly GetDefiningAssembly(MethodID callerId)
        {
            foreach (var assemblyProvider in _assemblies.Providers)
            {
                var generator = assemblyProvider.GetMethodGenerator(callerId);
                if (generator != null)
                    return _assemblies.GetTypeAssembly(assemblyProvider);
            }

            return null;
        }
        #endregion

        #endregion

        #region Event handlers

        /// <summary>
        /// Given reference has been removed from given <see cref="AssemblyProvider"/>. Removing assembly provider
        /// does not need this reference, however other providers may it still referenced
        /// </summary>
        /// <param name="assembly">Assembly which references changed</param>
        /// <param name="reference">Removed reference</param>
        private void _onReferenceRemoved(AssemblyProvider assembly, object reference)
        {
            reloadAssembly(assembly);
            throw new NotImplementedException("Info for lazy assembly unloading");
        }

        /// <summary>
        /// Given reference has been added into given <see cref="AssemblyProvider"/>. If the
        /// referenced assembly doesnot exists it has to be loaded
        /// </summary>
        /// <param name="assembly">Assembly which references changed</param>
        /// <param name="reference">Reference that has been added into assembly</param>
        private void _onReferenceAdded(AssemblyProvider assembly, object reference)
        {
            LoadReferenceAssembly(reference);
            throw new NotImplementedException("//TODO add reloading to after transaction action - reference could be added when assembly creation or asynchronously");
        }


        private void _onAssemblyInvalidation(AssemblyProvider assembly)
        {
            //remove invalidated assembly
            removeAssembly(assembly);

            if (_assemblies.IsRequired(assembly.Key))
                tryRecoverAssembly(assembly.Key);
            reloadAffectedAssemblies(assembly.Key);
        }

        private void _onRootAssemblyAdd(AssemblyProvider assembly)
        {
            //what to do with root assemblies
        }

        private void _onRootRegistered(AssemblyProvider assembly)
        {
            assembly.ComponentAdded += (compInfo) => _onComponentAdded(assembly, compInfo);

            var services = new TypeServices(assembly, this);
            assembly.TypeServices = services;


            if (AssemblyAdded != null)
                AssemblyAdded(assembly);
        }

        private void _onAssemblyRemove(AssemblyProvider assembly)
        {
            var componentsCopy = GetComponents(assembly).ToArray();
            foreach (var component in componentsCopy)
            {
                _onComponentRemoved(assembly, component);
            }

            assembly.Unload();
        }

        private void _onComponentAdded(AssemblyProvider assembly, ComponentInfo componentInfo)
        {
            componentInfo.DefiningAssembly = _assemblies.GetTypeAssembly(assembly);

            _assemblyComponents.Add(assembly, componentInfo);

            if (_components.ContainsKey(componentInfo.ComponentType))
                //TODO how to handle same components
                return;
            _components.Add(componentInfo.ComponentType, componentInfo);


            if (ComponentAdded != null)
                ComponentAdded(componentInfo);
        }

        private void _onComponentRemoved(AssemblyProvider assembly, ComponentInfo removedComponent)
        {
            _assemblyComponents.Remove(assembly, removedComponent);
            _components.Remove(removedComponent.ComponentType);

            if (ComponentRemoved != null)
                ComponentRemoved(removedComponent);
        }


        #endregion

        #region Private utility methods


        /// <summary>
        /// Find assembly that is already loaded. Mapping of assembly path is taken into consideration
        /// </summary>
        /// <param name="assemblyKey">Key of searched assembly</param>
        /// <returns>Found assembly if available, <c>null</c> otherwise</returns>
        private TypeAssembly findLoadedAssembly(object assemblyKey)
        {
            var assemblyPath = assemblyKey as string;
            if (assemblyPath != null)
            {
                //key is assembly path so it could be mapped
                var assembly = _assemblies.AccordingMappedFullpath(assemblyPath);

                if (assembly != null)
                    return assembly;
            }
            else
            {
                var assembly = _assemblies.GetProviderFromKey(assemblyKey);
                var typeAssembly = _assemblies.GetTypeAssembly(assembly);
                if (typeAssembly != null)
                    //assembly has been found
                    return typeAssembly;
            }
            return null;
        }

        private AssemblyProvider createAssembly(object key)
        {
            return _loader.CreateAssembly(key);
        }

        private InheritanceChain getChain(string typeName)
        {
            var typePath = new PathInfo(typeName);
            foreach (var assembly in _assemblies.Providers)
            {
                var inheritanceChain = assembly.GetInheritanceChain(typePath);

                if (inheritanceChain != null)
                {
                    return inheritanceChain;
                }
            }

            throw new NotSupportedException("For type: " + typeName + " there is no inheritance chain");
        }

        private MethodID tryDynamicResolve(TypeDescriptor dynamicInfo, MethodID method)
        {
            var result = dynamicExplicitResolve(method, dynamicInfo);

            if (result == null)
            {
                result = dynamicGenericResolve(method, dynamicInfo);
            }

            return result;
        }

        private GeneratorBase tryStaticResolve(MethodID method)
        {
            var result = staticExplicitResolve(method);

            if (result == null)
            {
                result = staticGenericResolve(method);
            }

            return result;
        }

        /// <summary>
        /// Resolve method generator with generic search on given method ID
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <returns>Generator for resolved method, or null, if there is no available generator</returns>
        private GeneratorBase staticGenericResolve(MethodID method)
        {
            var searchPath = Naming.GetMethodPath(method);
            if (!searchPath.HasGenericArguments)
                //there is no need for generic resolving
                return null;


            foreach (var assembly in _assemblies.Providers)
            {
                var generator = assembly.GetGenericMethodGenerator(method, searchPath);
                if (generator != null)
                {
                    return generator;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve method generator with exact method ID (no generic method searches)
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <returns>Generator for resolved method, or null, if there is no available generator</returns>
        private GeneratorBase staticExplicitResolve(MethodID method)
        {
            foreach (var assembly in _assemblies.Providers)
            {
                var generator = assembly.GetMethodGenerator(method);

                if (generator != null)
                {
                    return generator;
                }
            }
            return null;
        }

        private MethodID dynamicGenericResolve(MethodID method, InstanceInfo dynamicInfo)
        {
            var searchPath = Naming.GetMethodPath(method);
            if (!searchPath.HasGenericArguments)
                //there is no need for generic resolving
                return null;

            var methodSignature = Naming.ChangeDeclaringType(searchPath.Signature, method, true);
            var typePath = new PathInfo(dynamicInfo.TypeName);

            foreach (var assembly in _assemblies.Providers)
            {
                var implementation = assembly.GetGenericImplementation(method, searchPath, typePath);
                if (implementation != null)
                {
                    //implementation has been found
                    return implementation;
                }
            }
            return null;
        }

        private MethodID dynamicExplicitResolve(MethodID method, TypeDescriptor dynamicInfo)
        {
            foreach (var assembly in _assemblies.Providers)
            {
                var implementation = assembly.GetImplementation(method, dynamicInfo);
                if (implementation != null)
                {
                    //implementation has been found
                    return implementation;
                }
            }
            return null;
        }

        private IEnumerable<AssemblyProvider> resolveKeys(IEnumerable<object> keys)
        {
            foreach (var key in keys)
            {
                var resolved = _assemblies.GetProviderFromKey(key);

                if (resolved == null)
                    //assembly is not available
                    continue;

                yield return resolved;
            }
        }
        #endregion
    }
}
