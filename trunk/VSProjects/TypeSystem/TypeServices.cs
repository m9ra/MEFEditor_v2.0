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
        private readonly AssembliesManager _manager;

        internal TypeServices(AssembliesManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        public MethodSearcher CreateSearcher()
        {
            return _manager.CreateSearcher();
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

        public MethodID TryGetImplementation(InstanceInfo type, MethodID abstractMethod)
        {
            return _manager.TryGetImplementation(type, abstractMethod);
        }

        public TypeAssembly LoadAssembly(string assemblyPath)
        {
            return _manager.LoadAssembly(assemblyPath);
        }

        public void RegisterAssembly(string assemblyPath, AssemblyProvider assembly)
        {
            _manager.RegisterAssembly(assemblyPath, assembly);
        }

        public  MethodID GetStaticInitializer(InstanceInfo info)
        {
            return _manager.GetStaticInitializer(info);
        }
    }
}
