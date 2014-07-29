using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem.Core;
using MEFEditor.TypeSystem.Transactions;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Provides <see cref="MEFEditor.TypeSystem"/> services exposed to <see cref="AssemblyProvider"/> implementations.
    /// </summary>
    public class TypeServices
    {
        /// <summary>
        /// Representation of AppDomain where assemblies are stored.
        /// </summary>
        private readonly AssembliesManager _manager;

        /// <summary>
        /// Owner which uses current <see cref="TypeServices" />.
        /// </summary>
        private readonly AssemblyProvider _owner;

        /// <summary>
        /// References of <see cref="_owner" /> assembly.
        /// </summary>
        internal readonly ReferencedAssemblies References = new ReferencedAssemblies();

        /// <summary>
        /// Here are managed all <see cref="Transaction" /> objects.
        /// </summary>
        /// <value>The transactions.</value>
        internal TransactionManager Transactions { get { return _manager.Transactions; } }

        /// <summary>
        /// Currently available settings.
        /// </summary>
        /// <value>The settings.</value>
        public MachineSettings Settings { get { return _manager.Settings; } }

        /// <summary>
        /// Gets the actual code base full path.
        /// </summary>
        /// <value>The code base full path.</value>
        public string CodeBaseFullPath { get { return Settings.CodeBaseFullPath; } }

        /// <summary>
        /// Initialize <see cref="TypeServices" /> object that provides services from TypeSystem to given owner.
        /// </summary>
        /// <param name="owner">Owner which uses current <see cref="TypeServices" />.</param>
        /// <param name="manager">Representation of AppDomain where assemblies are stored.</param>
        internal TypeServices(AssemblyProvider owner, AssembliesManager manager)
        {
            _owner = owner;
            _manager = manager;

            //every assembly has to have runtime as prioritized reference
            //note that adding Runtime reference to itself is not a problem
            References.Add(Settings.Runtime);
            //assembly used for resolving is also owner itself
            References.Add(owner.Key);
        }

        #region Reference API

        /// <summary>
        /// Add reference for provided assembly. Referenced assembly is load if needed.
        /// </summary>
        /// <param name="reference">Reference representation used for assembly loading.</param>
        internal void AddReference(object reference)
        {
            References.Add(reference);

            //report reference add to manager - it will load coresponding assembly if possible
            //if loading fails reference will be added on registered change from outside
            //will cause reloading components
            _manager.ReportReferenceAdded(_owner, reference);
        }

        /// <summary>
        /// Remove reference from provided assembly. Referenced assembly may be unloaded.
        /// </summary>
        /// <param name="reference">Reference representation used for assembly unloading.</param>
        internal void RemoveReference(object reference)
        {
            //assembly corresponding to referece is cleaned up if needed lazily
            References.Remove(reference);

            //will cause reloading components
            _manager.ReportReferenceRemoved(_owner, reference);
        }

        #endregion

        #region Type inspection API

        /// <summary>
        /// Determines whether the specified instance is null.
        /// </summary>
        /// <param name="instance">The tested instance.</param>
        /// <returns><c>true</c> if the specified instance is null; otherwise, <c>false</c>.</returns>
        public bool IsNull(Instance instance)
        {
            return instance == null || (Runtime.Null.TypeInfo.Equals(instance.Info));
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies.
        /// </summary>
        /// <returns>Created method searcher.</returns>
        public MethodSearcher CreateSearcher()
        {
            return _manager.CreateSearcher(References);
        }

        /// <summary>
        /// Get concrete implementation of abstract (virtual,interface,..) method on given type.
        /// </summary>
        /// <param name="type">Type where concrete implementation is searched.</param>
        /// <param name="abstractMethod">Abstract method which implementation is searched.</param>
        /// <returns>Concrete implementation if available, <c>null</c> otherwise.</returns>
        public MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return _manager.TryGetImplementation(type, abstractMethod);
        }

        /// <summary>
        /// Determine that assignedType can be assigned into variable with targetTypeName without any conversion calls (implicit nor explicit)
        /// Only tests inheritance.
        /// </summary>
        /// <param name="targetTypeName">Name of target variable type.</param>
        /// <param name="assignedTypeName">Name of assigned type.</param>
        /// <returns>True if assigned type is assignable, false otherwise.</returns>
        public bool IsAssignable(string targetTypeName, string assignedTypeName)
        {
            return _manager.IsAssignable(targetTypeName, assignedTypeName);
        }

        /// <summary>
        /// Determine that assignedType can be assigned into variable with targetType without any conversion calls (implicit nor explicit)
        /// Only tests inheritance.
        /// </summary>
        /// <param name="targetType">Target variable type.</param>
        /// <param name="assignedType">Assigned type.</param>
        /// <returns><c>true</c> if assigned type is assignable, <c>false</c> otherwise.</returns>
        public bool IsAssignable(InstanceInfo targetType, InstanceInfo assignedType)
        {
            return IsAssignable(targetType.TypeName, assignedType.TypeName);
        }

        /// <summary>
        /// Get inheritance chain for given type.
        /// </summary>
        /// <param name="type">Type which inheritance chain is desired.</param>
        /// <returns>Founded inheritance chain if available, <c>null</c> otherwise.</returns>
        public InheritanceChain GetChain(TypeDescriptor type)
        {
            return _manager.GetChain(type);
        }

        /// <summary>
        /// Create inheritance chain for given type and subChains
        /// <remarks>This is used by <see cref="AssemblyProvider" /> to create information about inheritance</remarks>.
        /// </summary>
        /// <param name="type">Type which inheritance chain is created.</param>
        /// <param name="subChains"><see cref="InheritanceChain" /> of sub types.</param>
        /// <returns>Created chain.</returns>
        public InheritanceChain CreateChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            return _manager.CreateChain(type, subChains);
        }
        
        #endregion

        #region Component API

        /// <summary>
        /// Get <see cref="ComponentInfo" /> defined for given type.
        /// </summary>
        /// <param name="type">Type which component info is needed.</param>
        /// <returns><see cref="ComponentInfo" /> defined for type if available, <c>null</c> otherwise.</returns>
        public ComponentInfo GetComponentInfo(InstanceInfo type)
        {
            return _manager.GetComponentInfo(type);
        }

        /// <summary>
        /// Get components defined within given assembly.
        /// </summary>
        /// <param name="assembly">Assembly where components are searched.</param>
        /// <returns>Components defined within assembly.</returns>
        public IEnumerable<ComponentInfo> GetComponents(AssemblyProvider assembly)
        {
            return _manager.GetComponents(assembly);
        }


        #endregion

        #region Assembly API

        /// <summary>
        /// Register call handler that will be called instead of methods on registered <see cref="Instance" />.
        /// </summary>
        /// <param name="registeredInstance">Instance that is registered.</param>
        /// <param name="handler">Method that will be called.</param>
        public void RegisterCallHandler(Instance registeredInstance, DirectMethod handler)
        {
            _manager.RegisterCallHandler(registeredInstance, handler);
        }

        /// <summary>
        /// Reports invalidation of composition scheme.
        /// </summary>
        public void CompositionSchemeInvalidation()
        {
            _manager.CompositionSchemeInvalidation();
        }


        /// <summary>
        /// Invalidate all methods/types/beginning with given prefix from cache.
        /// </summary>
        /// <param name="invalidatedNamePrefix">Prefix used for method invalidation.</param>
        internal void Invalidate(string invalidatedNamePrefix)
        {
            _manager.Cache.Invalidate(invalidatedNamePrefix);
        }


        /// <summary>
        /// Get files that are present in given directory by taking assemblies mapping into consideration.
        /// </summary>
        /// <param name="directoryFullPath">Fullpath of directory which files will be retrieved.</param>
        /// <returns>Files that are present in directory according to virtual mapping.</returns>
        public IEnumerable<string> GetFiles(string directoryFullPath)
        {
            return _manager.GetFiles(directoryFullPath);
        }

        /// <summary>
        /// Load assembly for purposes of interpretation analysis. Assembly is automatically cached between multiple runs.
        /// Mapping of assemblies is take into consideration.
        /// </summary>
        /// <param name="assemblyPath">Path of loaded assembly.</param>
        /// <returns>Loaded assembly if available, <c>null</c> otherwise.</returns>
        public TypeAssembly LoadAssembly(string assemblyPath)
        {
            return _manager.LoadReferenceAssembly(assemblyPath);
        }


        /// <summary>
        /// Unload whole assembly from type system because of invalidation.
        /// </summary>
        /// <param name="provider">Representation of invalidate assembly.</param>
        internal void InvalidateAssembly(AssemblyProvider provider)
        {
            _manager.Unload(provider);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched.</param>
        /// <returns>Assembly where method is defined.</returns>
        public TypeAssembly GetDefiningAssembly(MethodID method)
        {
            return _manager.GetDefiningAssembly(method);
        }

        /// <summary>
        /// Get assembly which defines given type.
        /// </summary>
        /// <param name="type">Type which assembly is searched.</param>
        /// <returns>Assembly where type is defined.</returns>
        public TypeAssembly GetDefiningAssembly(InstanceInfo type)
        {
            return _manager.GetDefiningAssembly(type);
        }

        #endregion
    }
}
