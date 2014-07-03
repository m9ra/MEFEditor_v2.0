using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    public class AssemblyLoader : LoaderBase
    {
        /// <summary>
        /// Manager of loaded assemblies
        /// </summary>
        private readonly AssembliesManager _manager;

        private readonly List<AssemblyProviderFactory> _assemblyFactories = new List<AssemblyProviderFactory>();

        /// <summary>
        /// Services available
        /// </summary>
        public readonly AppDomainServices AppDomain;

        /// <summary>
        /// Currently available settings
        /// </summary>
        public MachineSettings Settings { get { return _manager.Settings; } }

        public AssemblyLoader(MachineSettings settings, params AssemblyProviderFactory[] factories)
        {
            _manager = new AssembliesManager(this,settings);
            _assemblyFactories.AddRange(factories);

            AppDomain = new AppDomainServices(_manager);
        }

        /// <summary>
        /// Load assembly defined by give assembly key into application domain
        /// </summary>
        /// <param name="assemblyKey">Key used for loading assembly</param>
        public AssemblyProvider LoadRoot(object assemblyKey)
        {
            var loadedAssembly = CreateOrGetAssembly(assemblyKey);

            if (loadedAssembly != null)
                _manager.LoadRoot(loadedAssembly);

            return loadedAssembly;
        }

        /// <summary>
        /// Load assembly defined by give assembly key into application domain
        /// </summary>
        /// <param name="assemblyKey">Key used for unloading assembly</param>
        public AssemblyProvider UnloadRoot(object assemblyKey)
        {
            return _manager.UnloadRoot(assemblyKey);
        }

        #region LoaderBase implementation

        /// </ inheritdoc>
        public override GeneratorBase StaticResolve(MethodID method)
        {
            return _manager.StaticResolve(method);
        }

        /// </ inheritdoc>
        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _manager.DynamicResolve(method, dynamicArgumentInfo);
        }

        /// </ inheritdoc>
        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _manager.GetComponentInfo(instanceInfo);
        }

        #endregion

        #region Assembly loading implementation

        /// <summary>
        /// Create assembly from given key
        /// </summary>
        /// <param name="assemblyKey">Key of created assembly</param>
        /// <returns>Created assembly if succesful, false otherwise</returns>
        internal AssemblyProvider CreateOrGetAssembly(object assemblyKey)
        {
            var assembly = _manager.FindLoadedAssemblyProvider(assemblyKey);
            if (assembly != null)
                return assembly;

            foreach (var factory in _assemblyFactories)
            {
                var createdAssembly = factory.Create(assemblyKey);

                if (createdAssembly != null)
                {
                    createdAssembly.Key = assemblyKey;
                    return createdAssembly;
                }
            }

            return null;
        }

        #endregion
    }
}
