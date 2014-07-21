using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Utilities;
using Analyzing;

using TypeSystem.Runtime;
using TypeSystem.Transactions;

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
        /// Stack for keeping correct ordering on system transactions
        /// </summary>
        private readonly Stack<Transaction> _systemTransactions = new Stack<Transaction>();

        /// <summary>
        /// Transaction used when interpreting is running
        /// </summary>
        private Transaction _interpetingTransaction;

        /// <summary>
        /// Components indexed by their defining assemblies
        /// </summary>
        private readonly MultiDictionary<AssemblyProvider, ComponentInfo> _assemblyComponents = new MultiDictionary<AssemblyProvider, ComponentInfo>();

        /// <summary>
        /// Components indexed by defining types
        /// </summary>
        private readonly Dictionary<InstanceInfo, ComponentInfo> _components = new Dictionary<InstanceInfo, ComponentInfo>();

        /// <summary>
        /// Cache used for storing method generators
        /// </summary>
        internal readonly MethodsCache Cache = new MethodsCache();

        /// <summary>
        /// Here are managed all <see cref="Transaction"/> objects
        /// </summary>
        internal readonly TransactionManager Transactions = new TransactionManager();

        /// <summary>
        /// Loader that is used for creating assemblies
        /// </summary>
        internal readonly AssemblyLoader Loader;

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

        /// <summary>
        /// Initialize new instance of <see cref="AssembliesManager"/> object
        /// </summary>
        /// <param name="loader">Loader that is used for loading of assemblies</param>
        /// <param name="settings">Settings used for interpretation</param>
        internal AssembliesManager(AssemblyLoader loader, MachineSettings settings)
        {
            Settings = settings;

            Loader = loader;

            _assemblies = new AssembliesStorage(this);
            _assemblies.OnRootAdd += _onRootAssemblyAdd;
            _assemblies.OnRootRemove += _onRootAssemblyRemoved;
            _assemblies.OnUnRegistered += _onAssemblyRemoved;
            _assemblies.OnRegistered += _onAssemblyRegistered;

            Settings.BeforeInterpretation += _beforeInterpretation;
            Settings.AfterInterpretation += _afterInterpretation;

            //runtime assembly has to be always present
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
        /// Immediately (not within after action) reload assemblies that are affected by given assembly key
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
            var componentsCopy = GetComponents(assembly).ToArray();
            foreach (var component in componentsCopy)
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

        #region Workflow transaction definitions

        /// <summary>
        /// Add reload assembly action to after actions of current transaction
        /// </summary>
        /// <param name="assembly">Assembly that will be reloaded</param>
        private void after_reloadAssembly(AssemblyProvider assembly)
        {
            addAfterAction(() => reloadAssembly(assembly), "ReloadAssembly", includedByReload, assembly);
        }

        /// <summary>
        /// Add components load action to after actions of current transaction
        /// </summary>
        /// <param name="assembly">Assembly which components will be loaded</param>
        private void after_loadComponents(AssemblyProvider assembly)
        {
            addAfterAction(() => loadComponents(assembly), "LoadComponents", includedByComponentsLoad, assembly);
        }

        /// <summary>
        /// Add after action to current transaction
        /// </summary>
        /// <param name="action"></param>
        /// <param name="name"></param>
        /// <param name="predicate"></param>
        /// <param name="keys"></param>
        private void addAfterAction(Action action, string name, IsIncludedPredicate predicate, params object[] keys)
        {
            var transactionAction = new TransactionAction(action, name, predicate, keys);
            Transactions.CurrentTransaction.AddAfterAction(transactionAction);
        }

        #region Transaction dependencies

        private bool includedByReload(TransactionAction action)
        {
            return
                action.Name == "ReloadAssembly" ||
                includedByComponentsInvalidation(action) ||
                includedByComponentsLoad(action);
        }

        private bool includedByComponentsInvalidation(TransactionAction action)
        {
            //TODO also include all component changes
            return action.Name == "InvalidateComponents";
        }

        private bool includedByComponentsLoad(TransactionAction action)
        {
            return action.Name == "LoadComponents";
        }

        #endregion

        #endregion

        #region Internal methods exposed for AssemblyLoader

        /// <summary>
        /// Resolve static (it means here non-virtual) method
        /// </summary>
        /// <param name="method">Method to be resolved</param>
        /// <returns>Generator of resolved method</returns>
        internal GeneratorBase StaticResolve(MethodID method)
        {
            var result = Cache.GetCachedGenerator(method, tryStaticResolve);

            if (result == null)
                Loader.AppDomain.Warning("Cannot find method: {0}", method);

            return result;
        }

        /// <summary>
        /// Resolve dynamic (it means here virtual) method
        /// </summary>
        /// <param name="method">Method to be resolved</param>
        /// <param name="dynamicArgumentInfo">Descriptors of arguments available during method invokation</param>
        /// <returns>Identifier of resolved method</returns>
        internal MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            //resolving of .NET objects depends only on called object type
            var calledObjectType = dynamicArgumentInfo[0] as TypeDescriptor;
            var methodImplementation = tryDynamicResolve(calledObjectType, method);

            if (methodImplementation == null)
                Loader.AppDomain.Warning("Doesn't have {0} implementation for: {1}", calledObjectType, method);

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
            if (chain == null)
                return false;
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

        /// <summary>
        /// Get components defined within given assembly and its referenced assemblies
        /// </summary>
        /// <param name="assembly">Assembly where components are searched</param>
        /// <returns>Components defined within assembly</returns>
        internal IEnumerable<ComponentInfo> GetReferencedComponents(AssemblyProvider assembly)
        {
            var result = new List<ComponentInfo>();
            foreach (var reference in resolveKeys(assembly.References))
            {
                result.AddRange(GetComponents(reference));
            }

            return result;
        }


        #endregion

        #region Assembly API

        /// <summary>
        /// Register call handler that will be called instead of methods on registered <see cref="Instance"/>
        /// </summary>
        /// <param name="registeredInstance">Instance that is registered</param>
        /// <param name="handler">Method that will be called</param>
        internal void RegisterCallHandler(Instance registeredInstance, DirectMethod handler)
        {
            var generator = new DirectGenerator(handler);
            Loader.RegisterInjectedGenerator(registeredInstance, generator);
        }

        /// <summary>
        /// Reports invalidation of composition scheme
        /// </summary>
        internal void CompositionSchemeInvalidation()
        {
            Loader.AppDomain.CompositionSchemeInvalidation();
        }

        /// <summary>
        /// Get files that are present in given directory by taking assemblies mapping into consideration
        /// </summary>
        /// <param name="directoryFullPath">Fullpath of directory which files will be retrieved</param>
        /// <returns>Files that are present in directory according to virtual mapping</returns>
        internal IEnumerable<string> GetFiles(string directoryFullPath)
        {
            var listed = new HashSet<string>();

            //get files that are added by virtual mapping
            foreach (var assembly in _assemblies.Providers)
            {
                if (!assembly.FullPathMapping.StartsWith(directoryFullPath))
                    //directory has to match begining of path
                    continue;

                var mappedDirectory = Path.GetDirectoryName(assembly.FullPathMapping);
                if (mappedDirectory == directoryFullPath && listed.Add(assembly.FullPathMapping))
                    //virtual mapping match
                    yield return assembly.FullPathMapping;
            }

            if (Directory.Exists(directoryFullPath))
            {
                var realFiles = Directory.GetFiles(directoryFullPath);

                //get real files filtered by mapped assemblies
                foreach (var realFile in realFiles)
                {
                    if (_assemblies.ContainsRealFile(realFile))
                        //assemblies are added according to their mapping
                        continue;

                    if (listed.Contains(realFile))
                        //file has been overriden by some mapping
                        continue;

                    yield return realFile;
                }
            }
        }

        /// <summary>
        /// Load assembly for purposes of interpretation analysis. Assembly is automatically cached between multiple runs.
        /// Mapping of assemblies is take into consideration
        /// <remarks>TODO: Reloading affected assemblies is not processed. Should be?</remarks>
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
        /// <param name="loadedAssembly">Asembly that is loaded</param>
        internal void LoadRoot(AssemblyProvider loadedAssembly)
        {
            //adding will fire appropriate handlers
            _assemblies.AddRoot(loadedAssembly);
        }

        /// <summary>
        /// Unload assembly from AppDomain roots
        /// </summary>
        /// <param name="unloadedAssemblyKey">Asembly that is unloaded</param>
        internal AssemblyProvider UnloadRoot(object unloadedAssemblyKey)
        {
            //removing will fire appropriate handlers
            var provider = _assemblies.FindProviderFromKey(unloadedAssemblyKey);
            _assemblies.RemoveRoot(provider);
            return provider;
        }


        /// <summary>
        /// Completely unload given assembly
        /// </summary>
        /// <param name="assembly">Assembly to unload</param>
        internal void Unload(AssemblyProvider assembly)
        {
            _assemblies.Remove(assembly);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly where method is defined</returns>
        internal TypeAssembly GetDefiningAssembly(MethodID method)
        {
            var definingAssemblyProvider = GetDefiningAssemblyProvider(method);
            if (definingAssemblyProvider == null)
                return null;

            return _assemblies.GetTypeAssembly(definingAssemblyProvider);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly provider where method is defined</returns>
        internal AssemblyProvider GetDefiningAssemblyProvider(MethodID method)
        {
            var definingAssemblyProvider = Cache.GetCachedDefiningAssembly(method, (x) =>
            {
                foreach (var assemblyProvider in _assemblies.Providers)
                {
                    var generator = assemblyProvider.GetMethodGenerator(method);
                    if (generator != null)
                        return assemblyProvider;
                }

                return null;
            });
            return definingAssemblyProvider;
        }

        /// <summary>
        /// Find assembly provider that is already loaded
        /// </summary>
        /// <param name="key">Key defining the assembly provider</param>
        /// <returns>Loaded assebmly provided if available, <c>null</c> otherwise</returns>
        internal AssemblyProvider FindLoadedAssemblyProvider(object key)
        {
            return _assemblies.FindProviderFromKey(key);
        }

        #endregion

        #endregion

        #region Event handlers

        /// <summary>
        /// Handler fired before interpretation is started
        /// </summary>
        private void _beforeInterpretation()
        {
            _interpetingTransaction = Transactions.StartNew("Interpreting");
        }

        /// <summary>
        /// Handler fired after interpretation is completed
        /// </summary>
        private void _afterInterpretation()
        {
            _interpetingTransaction.Commit();

            _interpetingTransaction = null;
        }

        /// <summary>
        /// Given reference has been removed from given <see cref="AssemblyProvider"/>. Removing assembly provider
        /// does not need this reference, however other providers may it still referenced
        /// </summary>
        /// <param name="assembly">Assembly which references changed</param>
        /// <param name="reference">Removed reference</param>
        private void _onReferenceRemoved(AssemblyProvider assembly, object reference)
        {
            after_reloadAssembly(assembly);
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
            after_reloadAssembly(assembly);
        }

        /// <summary>
        /// Handler called for assembly that is invalidated
        /// </summary>
        /// <param name="assembly">Invalidated assembly</param>
        private void _onAssemblyInvalidation(AssemblyProvider assembly)
        {
            //remove invalidated assembly
            removeAssembly(assembly);

            if (_assemblies.IsRequired(assembly.Key))
                tryRecoverAssembly(assembly.Key);

            reloadAffectedAssemblies(assembly.Key);
        }

        /// <summary>
        /// Root assembly has been added
        /// </summary>
        /// <param name="assembly">Added assembly</param>
        private void _onRootAssemblyAdd(AssemblyProvider assembly)
        {
            //what to do with root assemblies
        }

        /// <summary>
        /// Root assembly has been removed
        /// </summary>
        /// <param name="assembly">Removed assembly</param>
        private void _onRootAssemblyRemoved(AssemblyProvider assembly)
        {
            //what to do with root assemblies
        }

        /// <summary>
        /// Assembly has been registered (every assembly has to be registered exactly one if loaded)
        /// </summary>
        /// <param name="assembly">Registered assembly</param>
        private void _onAssemblyRegistered(AssemblyProvider assembly)
        {
            startTransaction("Registering assembly: " + assembly.Name);
            assembly.ComponentAdded += (compInfo) => _onComponentAdded(assembly, compInfo);
            assembly.ComponentRemoved += (compInfo) => _onComponentRemoved(assembly, compInfo);

            var services = new TypeServices(assembly, this);
            assembly.TypeServices = services;

            after_loadComponents(assembly);

            try
            {
                if (AssemblyAdded != null)
                    AssemblyAdded(assembly);
            }
            finally
            {
                commitTransaction();
            }
        }

        /// <summary>
        /// Assembly that has been removed completely from <see cref="AssembliesManager"/>
        /// </summary>
        /// <param name="assembly">Removed assembly</param>
        private void _onAssemblyRemoved(AssemblyProvider assembly)
        {
            startTransaction("Unregistering assembly: " + assembly.Name);

            try
            {
                var componentsCopy = GetComponents(assembly).ToArray();
                foreach (var component in componentsCopy)
                {
                    _onComponentRemoved(assembly, component);
                }

                assembly.Unload();

                if (AssemblyRemoved != null)
                    AssemblyRemoved(assembly);
            }
            finally
            {
                commitTransaction();
            }
        }

        /// <summary>
        /// Handler fired whenever component is added
        /// </summary>
        /// <param name="assembly">Assembly where component has been discovered</param>
        /// <param name="componentInfo">Information about component</param>
        private void _onComponentAdded(AssemblyProvider assembly, ComponentInfo componentInfo)
        {
            componentInfo.DefiningAssembly = _assemblies.GetTypeAssembly(assembly);

            if (!_components.ContainsKey(componentInfo.ComponentType))
            {
                _components.Add(componentInfo.ComponentType, componentInfo);
            }

            if (!_assemblyComponents.Add(assembly, componentInfo))
                //nothing has been added
                return;

            if (ComponentAdded != null)
                ComponentAdded(componentInfo);
        }

        /// <summary>
        /// Handler fired whenever component is removed
        /// </summary>
        /// <param name="assembly">Assembly where component has been defined</param>
        /// <param name="removedComponent">Information about removed component</param>
        private void _onComponentRemoved(AssemblyProvider assembly, ComponentInfo removedComponent)
        {
            _assemblyComponents.Remove(assembly, removedComponent);
            _components.Remove(removedComponent.ComponentType);

            if (ComponentRemoved != null)
                ComponentRemoved(removedComponent);
        }


        #endregion

        #region Transaction system

        /// <summary>
        /// Strat transaction with given description. Expect safe usage - for TypeSystem purposes only
        /// </summary>
        /// <param name="description">Description of started transaction</param>
        private void startTransaction(string description)
        {
            var transaction = Transactions.StartNew(description);
            _systemTransactions.Push(transaction);
        }

        /// <summary>
        /// Commit lastly opened system transaction. Has to be called correctly for every 
        /// <see cref="startTransaction"/> call.
        /// </summary>
        private void commitTransaction()
        {
            var transaction = _systemTransactions.Pop();
            transaction.Commit();
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
                var assembly = _assemblies.FindProviderFromKey(assemblyKey);
                var typeAssembly = _assemblies.GetTypeAssembly(assembly);
                if (typeAssembly != null)
                    //assembly has been found
                    return typeAssembly;
            }
            return null;
        }

        /// <summary>
        /// Create assembly provider from given key, by using <see cref="AssemblyLoader"/>
        /// </summary>
        /// <param name="key">Key that is used as definition for assembly creation</param>
        /// <returns>Created assembly provider</returns>
        private AssemblyProvider createAssembly(object key)
        {
            var assembly = Loader.CreateOrGetAssembly(key);
            return assembly;
        }

        /// <summary>
        /// Get inheritance chain for given type
        /// </summary>
        /// <param name="typeName">name of type which inheritance chain is required</param>
        /// <returns>Created inheritance chain</returns>
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

            return null;
        }

        /// <summary>
        /// Try to resolve dynamic (generic/non-generic) method according to descriptor of called object
        /// </summary>
        /// <param name="calledObjectDescriptor">Descriptor of called object</param>
        /// <param name="method">Method that is resolved</param>
        /// <returns>Resolved method if available, <c>null</c> othewrise</returns>
        private MethodID tryDynamicResolve(TypeDescriptor calledObjectDescriptor, MethodID method)
        {
            var result = tryDynamicExplicitResolve(calledObjectDescriptor, method);

            if (result == null)
            {
                result = tryDynamicGenericResolve(calledObjectDescriptor, method);
            }

            return result;
        }

        /// <summary>
        /// Try to resolve static (generic/non-generic) method
        /// </summary>
        /// <param name="method">Method that is resolved</param>
        /// <param name="definingAssembly">Assembly where method is defined</param>
        /// <returns>Resolved method if available, <c>null</c> otherwise</returns>
        private GeneratorBase tryStaticResolve(MethodID method, out AssemblyProvider definingAssembly)
        {
            var result = tryStaticExplicitResolve(method, out definingAssembly);

            if (result == null)
            {
                result = tryStaticGenericResolve(method, out definingAssembly);
            }

            return result;
        }

        /// <summary>
        /// Resolve method generator with generic search on given method ID
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <param name="definingAssembly">Assembly where method is defined</param>
        /// <returns>Generator for resolved method, or null, if there is no available generator</returns>
        private GeneratorBase tryStaticGenericResolve(MethodID method, out AssemblyProvider definingAssembly)
        {
            definingAssembly = null;
            var searchPath = Naming.GetMethodPath(method);
            if (!searchPath.HasGenericArguments)
                //there is no need for generic resolving
                return null;


            foreach (var assembly in _assemblies.Providers)
            {
                var generator = assembly.GetGenericMethodGenerator(method, searchPath);
                if (generator != null)
                {
                    definingAssembly = assembly;
                    return generator;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve method generator with exact method ID (no generic method searches)
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <param name="definingAssembly">Assembly where method is defined</param>
        /// <returns>Generator for resolved method, or null, if there is no available generator</returns>
        private GeneratorBase tryStaticExplicitResolve(MethodID method, out AssemblyProvider definingAssembly)
        {
            foreach (var assembly in _assemblies.Providers)
            {
                var generator = assembly.GetMethodGenerator(method);

                if (generator != null)
                {
                    definingAssembly = assembly;
                    return generator;
                }
            }
            definingAssembly = null;
            return null;
        }

        /// <summary>
        /// Try to resolve dynamic generic method according to descriptor of called object
        /// </summary>
        /// <param name="calledObjectDescriptor">Descriptor of called object</param>
        /// <param name="method">Method that is resolved</param>
        /// <returns>Resolved method if available, <c>null</c> othewrise</returns>
        private MethodID tryDynamicGenericResolve(TypeDescriptor calledObjectDescriptor, MethodID method)
        {
            var searchPath = Naming.GetMethodPath(method);
            if (!searchPath.HasGenericArguments)
                //there is no need for generic resolving
                return null;


            var methodSignature = Naming.ChangeDeclaringType(searchPath.Signature, method, true);
            var typePath = new PathInfo(calledObjectDescriptor.TypeName);

            var implementersQueue = new Queue<PathInfo>();
            implementersQueue.Enqueue(typePath);
            while (implementersQueue.Count > 0)
            {
                var implementer = implementersQueue.Dequeue();
                foreach (var assembly in _assemblies.Providers)
                {
                    PathInfo alternativeImplementer;
                    var implementation = assembly.GetGenericImplementation(method, searchPath, implementer, out alternativeImplementer);
                    if (implementation != null)
                    {
                        //implementation has been found
                        return implementation;
                    }

                    if (alternativeImplementer != null)
                        implementersQueue.Enqueue(alternativeImplementer);
                }
            }
            return null;
        }

        /// <summary>
        /// Try to resolve dynamic method according to descriptor of called object
        /// </summary>
        /// <param name="calledObjectDescriptor">Descriptor of called object</param>
        /// <param name="method">Method that is resolved</param>
        /// <returns>Resolved method if available, <c>null</c> othewrise</returns>
        private MethodID tryDynamicExplicitResolve(TypeDescriptor calledObjectDescriptor, MethodID method)
        {
            var implementers = new Queue<TypeDescriptor>();
            implementers.Enqueue(calledObjectDescriptor);
            while (implementers.Count > 0)
            {
                var implementer = implementers.Dequeue();

                foreach (var assembly in _assemblies.Providers)
                {
                    TypeDescriptor alternativeImplementer;
                    var implementation = assembly.GetImplementation(method, implementer, out alternativeImplementer);
                    if (implementation != null)
                    {
                        //implementation has been found
                        return implementation;
                    }

                    if (alternativeImplementer != null)
                        implementers.Enqueue(alternativeImplementer);
                }
            }
            return null;
        }

        /// <summary>
        /// Resolve enumeration of keys into enumeration of <see cref="AssemblyProvider"/> objects
        /// </summary>
        /// <param name="keys">Keys to be resolved</param>
        /// <returns>Resolved enumeration</returns>
        private IEnumerable<AssemblyProvider> resolveKeys(IEnumerable<object> keys)
        {
            foreach (var key in keys)
            {
                var resolved = _assemblies.FindProviderFromKey(key);

                if (resolved == null)
                    //assembly is not available
                    continue;

                yield return resolved;
            }
        }

        #endregion
    }
}
