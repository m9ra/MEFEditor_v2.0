using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.TypeSystem.Core;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Loader of assemblies used by <see cref="AppDomainServices"/>.
    /// </summary>
    public class AssemblyLoader : LoaderBase
    {
        /// <summary>
        /// Manager of loaded assemblies.
        /// </summary>
        private readonly AssembliesManager _manager;

        /// <summary>
        /// Currently available factories of <see cref="AssemblyProvider" />.
        /// </summary>
        private readonly AssemblyProviderFactory[] _assemblyFactories;

        /// <summary>
        /// Generators that are registered in current context for given instances.
        /// </summary>
        private readonly Dictionary<Instance, GeneratorBase> _overridingGenerators = new Dictionary<Instance, GeneratorBase>();

        /// <summary>
        /// Generator used for handling calls on null value.
        /// </summary>
        private readonly DirectGenerator _nullCallHandler;

        /// <summary>
        /// Available app domain services.
        /// </summary>
        public readonly AppDomainServices AppDomain;

        /// <summary>
        /// Currently available settings.
        /// </summary>
        /// <value>The settings.</value>
        public MachineSettings Settings { get { return _manager.Settings; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoader" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="factories">The factories used for loading assemblies.</param>
        public AssemblyLoader(MachineSettings settings, params AssemblyProviderFactory[] factories)
        {
            _manager = new AssembliesManager(this, settings);
            _assemblyFactories = factories.ToArray();
            _nullCallHandler = new DirectGenerator(nullCallHandler);

            AppDomain = new AppDomainServices(_manager);


            //register cleaning of overriding generators
            Settings.BeforeInterpretation += () => _overridingGenerators.Clear();
        }

        #region Public API

        /// <summary>
        /// Load assembly defined by give assembly key into application domain.
        /// </summary>
        /// <param name="assemblyKey">Key used for loading assembly.</param>
        /// <returns>AssemblyProvider.</returns>
        public AssemblyProvider LoadRoot(object assemblyKey)
        {
            var loadedAssembly = CreateOrGetAssembly(assemblyKey);

            if (loadedAssembly != null)
                _manager.LoadRoot(loadedAssembly);

            return loadedAssembly;
        }

        /// <summary>
        /// Load assembly defined by give assembly key into application domain.
        /// </summary>
        /// <param name="assemblyKey">Key used for unloading assembly.</param>
        /// <returns>AssemblyProvider.</returns>
        public AssemblyProvider UnloadRoot(object assemblyKey)
        {
            return _manager.UnloadRoot(assemblyKey);
        }

        /// <summary>
        /// Unloads the assemblies.
        /// </summary>
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
        /// Unloads assembly with specified key.
        /// </summary>
        public void Unload(object key)
        {
            var assemblyProvider=_manager.FindLoadedAssemblyProvider(key);
            if(assemblyProvider==null)
                //there is nothing to do
                return;

            _manager.Unload(assemblyProvider);
        }

        /// <summary>
        /// Get <see cref="ComponentInfo" /> for given <see cref="InstanceInfo" />.
        /// </summary>
        /// <param name="instanceInfo"><see cref="InstanceInfo" /> which defines type of component.</param>
        /// <returns><see cref="ComponentInfo" /> if available, <c>null</c> otherwise.</returns>
        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _manager.GetComponentInfo(instanceInfo);
        }

        #endregion

        #region LoaderBase implementation

        /// <summary>
        /// Resolve method with static argument info.
        /// </summary>
        /// <param name="method">Resolved method.</param>
        /// <returns>Resolved method name.</returns>
        public override GeneratorBase StaticResolve(MethodID method)
        {
            return _manager.StaticResolve(method);
        }

        /// <summary>
        /// Resolve method with dynamic argument info.
        /// </summary>
        /// <param name="method">Resolved method.</param>
        /// <param name="dynamicArgumentInfo">Dynamic argument info, collected from argument instances.</param>
        /// <returns>Resolved method which will be asked for generator by StaticResolve.</returns>
        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _manager.DynamicResolve(method, dynamicArgumentInfo);
        }

        /// <summary>
        /// When overridden it can inject any generator for any method. Injected generator
        /// wont be binded with <see cref="MethodID" /> in methods cache.
        /// </summary>
        /// <param name="name">Name of resolved method.</param>
        /// <param name="argumentValues">Arguments of resolved method.</param>
        /// <returns><c>null</c> if there is no injected generator, injected generator otherwise.</returns>
        public override GeneratorBase GetOverridingGenerator(MethodID name, Instance[] argumentValues)
        {
            if (argumentValues.Length == 0)
                return null;

            var thisArgument = argumentValues[0];

            GeneratorBase generator;
            _overridingGenerators.TryGetValue(thisArgument, out generator);

            if (thisArgument.Info.TypeName == Runtime.Null.TypeInfo.TypeName)
                return _nullCallHandler;

            return generator;
        }

        /// <summary>
        /// Register injected generator for given instance. All incoming
        /// calls will be replaced with instructions of given generator.
        /// </summary>
        /// <param name="registeredInstance">Instance which generator is injected.</param>
        /// <param name="generator">Injected generator.</param>
        internal void RegisterInjectedGenerator(Instance registeredInstance, DirectGenerator generator)
        {
            _overridingGenerators[registeredInstance] = generator;
        }

        #endregion

        #region Assembly loading implementation

        /// <summary>
        /// Create assembly from given key.
        /// </summary>
        /// <param name="assemblyKey">Key of created assembly.</param>
        /// <returns>Created assembly if successful, false otherwise.</returns>
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

        #region Private utilities

        /// <summary>
        /// Handler called whenever call on null instance is detected.
        /// </summary>
        /// <param name="context">Context used during analysis.</param>
        private void nullCallHandler(AnalyzingContext context)
        {
            AppDomain.Log("WARNING", "Call on null instance detected: " + context.CurrentCall.Name);

            foreach (var argument in context.CurrentArguments.Skip(1))
            {
                if (argument.IsDirty)
                    continue;

                if (Settings.IsDirect(argument.Info))
                    continue;

                if (argument.Info.TypeName == Runtime.Null.TypeInfo.TypeName)
                    continue;

                context.SetDirty(argument);
                AppDomain.Log("WARNING", "Setting dirty flag for {0}", argument);
            }
        }

        #endregion

    }
}
