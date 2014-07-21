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

        /// <summary>
        /// Currently available factories of <see cref="AssemblyProvider"/>
        /// </summary>
        private readonly AssemblyProviderFactory[] _assemblyFactories;

        /// <summary>
        /// Generators that are registered in current context for given instances
        /// </summary>
        private readonly Dictionary<Instance, GeneratorBase> _overridingGenerators = new Dictionary<Instance, GeneratorBase>();

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
            _manager = new AssembliesManager(this, settings);
            _assemblyFactories = factories.ToArray();

            AppDomain = new AppDomainServices(_manager);

            //register cleaning of overriding generators
            Settings.BeforeInterpretation += () => _overridingGenerators.Clear();
        }
        #region Public API

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

        public void UnloadAssemblies()
        {
            var assembliesCopy = _manager.Assemblies.ToArray();
            foreach (var assembly in assembliesCopy)
            {
                if (assembly == _manager.Runtime)
                    //we dont want to unload runtime
                    continue;
                _manager.Unload(assembly);
            }
        }

        /// <summary>
        /// Get <see cref="ComponentInfo"/> for given <see cref="InstanceInfo"/>
        /// </summary>
        /// <param name="instanceInfo"><see cref="InstanceInfo"/> which defines type of component</param>
        /// <returns><see cref="ComponentInfo"/> if available, <c>null</c> otherwise.</returns>
        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _manager.GetComponentInfo(instanceInfo);
        }

        #endregion

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
        public override GeneratorBase GetOverridingGenerator(MethodID name, Instance[] argumentValues)
        {
            if (argumentValues.Length == 0)
                return null;

            GeneratorBase generator;
            _overridingGenerators.TryGetValue(argumentValues[0], out generator);
            return generator;
        }

        /// <summary>
        /// Register injected generator for given instance. All incomming
        /// calls will be replaced with instructions of given generator.
        /// </summary>
        /// <param name="registeredInstance">Instance which generator is injected</param>
        /// <param name="generator">Injected generator</param>
        internal void RegisterInjectedGenerator(Instance registeredInstance, DirectGenerator generator)
        {
            _overridingGenerators[registeredInstance] = generator;
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
