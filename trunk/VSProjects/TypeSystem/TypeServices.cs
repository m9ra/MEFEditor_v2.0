using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

using Analyzing;

namespace TypeSystem
{
    /// <summary>
    /// Provides access to information about loaded assemblies
    /// </summary>
    public class TypeServices
    {
        /// <summary>
        /// Representation of AppDomain where assemblies are stored
        /// </summary>
        private readonly AssembliesManager _manager;

        /// <summary>
        /// Owner which uses current <see cref="TypeServices"/>
        /// </summary>
        private readonly AssemblyProvider _owner;

        /// <summary>
        /// References of <see cref="_owner"/> assembly
        /// </summary>
        internal readonly ReferencedAssemblies References = new ReferencedAssemblies();

        /// <summary>
        /// Currently available settings
        /// </summary>
        public MachineSettings Settings { get { return _manager.Settings; } }

        /// <summary>
        /// Initialize <see cref="TypeServices"/> object that provides services from TypeSystem to given owner
        /// </summary>
        /// <param name="owner">Owner which uses current <see cref="TypeServices"/></param>
        /// <param name="manager">Representation of AppDomain where assemblies are stored</param>
        internal TypeServices(AssemblyProvider owner, AssembliesManager manager)
        {
            _owner = owner;
            _manager = manager;

            //every assembly has to have runtime as prioritized reference
            //note that adding Runtime reference to itself is not a problem
            References.Add(Settings.Runtime);
            //assembly used for resolving is also owner itself
            References.Add(owner);
        }

        #region Reference API

        /// <summary>
        /// Add reference for provided assembly. Referenced assembly is load if needed
        /// </summary>
        /// <param name="reference">Reference representation used for assembly loading</param>
        internal void AddReference(object reference)
        {
            //loading of assemblies is processed before compositoin point start
            References.Add(reference);
            _manager.ReportReferenceAdded(_owner, reference);
        }

        /// <summary>
        /// Remove reference from provided assembly. Referenced assembly may be unloaded
        /// </summary>
        /// <param name="reference">Reference representation used for assembly unloading</param>
        internal void RemoveReference(object reference)
        {
            //removed assemblies are cleaned up if needed lazily
            References.Remove(reference);
            _manager.ReportReferenceRemoved(_owner, reference);
        }

        #endregion

        #region Type inspection API

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        public MethodSearcher CreateSearcher()
        {
            return _manager.CreateSearcher(References);
        }

        /// <summary>
        /// Get concrete implementation of abstract (virtual,interface,..) method on given type
        /// </summary>
        /// <param name="type">Type where concrete implementation is searched</param>
        /// <param name="abstractMethod">Abstract method which implementation is searched</param>
        /// <returns>Concreate implementation if available, <c>null</c> otherwise</returns>
        public MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return _manager.TryGetImplementation(type, abstractMethod);
        }

        /// <summary>
        /// Determine that assignedType can be assigned into variable with targetTypeName without any conversion calls (implicit nor explicit)
        /// Only tests inheritance
        /// </summary>
        /// <param name="targetTypeName">Name of target variable type</param>
        /// <param name="assignedTypeName">Name of assigned type</param>
        /// <returns>True if assigned type is assignable, false otherwise</returns>
        public bool IsAssignable(string targetTypeName, string assignedTypeName)
        {
            return _manager.IsAssignable(targetTypeName, assignedTypeName);
        }

        /// <summary>
        /// Determine that assignedType can be assigned into variable with targetType without any conversion calls (implicit nor explicit)
        /// Only tests inheritance
        /// </summary>
        /// <param name="targetType">Target variable type</param>
        /// <param name="assignedType">Assigned type</param>
        /// <returns><c>true</c> if assigned type is assignable, <c>false</c> otherwise</returns>
        public bool IsAssignable(InstanceInfo targetType, InstanceInfo assignedType)
        {
            return IsAssignable(targetType.TypeName, assignedType.TypeName);
        }

        /// <summary>
        /// Get inheritance chain for given type
        /// </summary>
        /// <param name="type">Type which inheritance chain is desired</param>
        /// <returns>Founded inheritance chain if available, <c>null</c> otherwise</returns>
        public InheritanceChain GetChain(TypeDescriptor type)
        {
            return _manager.GetChain(type);
        }

        /// <summary>
        /// Create inheritance chain for given type and subChains
        /// <remarks>This is used by <see cref="AssemblyProvider"/> to create information about inheritance</remarks>
        /// </summary>
        /// <param name="type">Type which inheritance chain is created</param>
        /// <param name="subChains"><see cref="InheritanceChains"/> of sub types</param>
        /// <returns>Created chain</returns>
        public InheritanceChain CreateChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            return _manager.CreateChain(type, subChains);
        }


        #endregion

        #region Component API

        /// <summary>
        /// Get <see cref="ComponentInfo"/> defined for given type.
        /// </summary>
        /// <param name="type">Type which component info is needed</param>
        /// <returns><see cref="ComponentInfo"/> defined for type if available, <c>false</c> otherwise</returns>
        public ComponentInfo GetComponentInfo(InstanceInfo type)
        {
            return _manager.GetComponentInfo(type);
        }

        /// <summary>
        /// Get components defined within given assembly
        /// </summary>
        /// <param name="assembly">Assembly where components are searched</param>
        /// <returns>Components defined within assembly</returns>
        public IEnumerable<ComponentInfo> GetComponents(AssemblyProvider assembly)
        {
            return _manager.GetComponents(assembly);
        }


        #endregion

        #region Assembly API

        /// <summary>
        /// Get files that are present in given directory by taking assemblies mapping into consideration
        /// </summary>
        /// <param name="directoryFullPath">Fullpath of directory which files will be retrieved</param>
        /// <returns>Files that are present in directory according to virtual mapping</returns>
        public IEnumerable<string> GetFiles(string directoryFullPath)
        {
            return _manager.GetFiles(directoryFullPath);
        }

        /// <summary>
        /// Load assembly for purposes of interpretation analysis. Assembly is automatically cached between multiple runs.
        /// Mapping of assemblies is take into consideration
        /// </summary>
        /// <param name="assemblyPath">Path of loaded assembly</param>
        /// <returns>Loaded assembly if available, <c>null</c> otherwise</returns>
        public TypeAssembly LoadAssembly(string assemblyPath)
        {
            //TODO load only for purposes of single composition point
            return _manager.LoadReferenceAssembly(assemblyPath);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly where method is defined</returns>
        public TypeAssembly GetDefiningAssembly(MethodID method)
        {
            return _manager.GetDefiningAssembly(method);
        }

        #endregion

    }
}
