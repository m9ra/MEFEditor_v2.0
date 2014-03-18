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
        private readonly AssemblyProvider _owner;

        private readonly AssembliesManager _manager;

        private readonly ReferencedAssemblies _references = new ReferencedAssemblies();

        /// <summary>
        /// Currently available settings
        /// </summary>
        public MachineSettings Settings { get { return _manager.Settings; } }

        internal TypeServices(AssemblyProvider owner, AssembliesManager manager)
        {
            _owner = owner;
            _manager = manager;

            //every assembly has to have runtime as prioritized reference
            //note that adding Runtime reference to itself is not a problem
            _references.Add(Settings.Runtime);
            //assembly used for resolving is also owner itself
            _references.Add(owner);
        }

        /// <summary>
        /// Add reference for provided assembly. Referenced assembly is load if needed
        /// </summary>
        /// <param name="reference">Reference representation used for assembly loading</param>
        internal void AddReference(object reference)
        {
            var assembly = _manager.LoadReference(reference);
            _references.Add(assembly);
        }

        /// <summary>
        /// Remove reference from provided assembly. Referenced assembly may be unloaded
        /// </summary>
        /// <param name="reference">Reference representation used for assembly unloading</param>
        internal void RemoveReference(object reference)
        {
            var assembly = _manager.UnLoadReference(reference);
            _references.Remove(assembly);
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        public MethodSearcher CreateSearcher()
        {
            return _manager.CreateSearcher(_references);
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

        public bool IsAssignable(InstanceInfo targetType, InstanceInfo assignedType)
        {
            return IsAssignable(targetType.TypeName, assignedType.TypeName);
        }

        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _manager.GetComponentInfo(instanceInfo);
        }

        public MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return _manager.TryGetImplementation(type, abstractMethod);
        }

        public TypeAssembly LoadAssembly(string assemblyPath)
        {
            //TODO load only for purposes of single composition point
            return _manager.LoadAssembly(assemblyPath);
        }

        public void RegisterAssembly(AssemblyProvider assembly)
        {
            //TODO it doesn't belong here
            _manager.RegisterAssembly(assembly);
        }
        
        public TypeAssembly DefiningAssembly(MethodID callerId)
        {
            return _manager.DefiningAssembly(callerId);
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

        /// <summary>
        /// Get files that are present in given directory
        /// </summary>
        /// <param name="directoryFullPath"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFiles(string directoryFullPath)
        {
            return _manager.GetFiles(directoryFullPath);
        }

        public InheritanceChain CreateChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            return _manager.CreateChain(type, subChains);
        }

        public InheritanceChain GetChain(TypeDescriptor type)
        {
            return _manager.GetChain(type);
        }

    }
}
