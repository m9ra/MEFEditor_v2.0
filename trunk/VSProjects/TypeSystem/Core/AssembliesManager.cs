using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using Utilities;

namespace TypeSystem.Core
{
    class AssembliesManager
    {
        readonly AssemblyCollection _assembliesCollection;

        readonly MachineSettings _settings;

        /// <summary>
        /// In these assemblies are searched generators
        /// <remarks>May differ from method searcher providing assemblies - that are based on assembly references</remarks>
        /// </summary>
        readonly List<AssemblyProvider> _assemblies = new List<AssemblyProvider>();

        readonly TypeServices _services;

        readonly MultiDictionary<AssemblyProvider, ComponentInfo> _assemblyComponents = new MultiDictionary<AssemblyProvider, ComponentInfo>();

        readonly Dictionary<InstanceInfo, ComponentInfo> _components = new Dictionary<InstanceInfo, ComponentInfo>();

        readonly Dictionary<string, TypeAssembly> _loadedAssemblies = new Dictionary<string, TypeAssembly>();

        internal AssembliesManager(AssemblyCollection assemblies, MachineSettings settings)
        {
            _services = new TypeServices(this);
            _settings = settings;

            _assembliesCollection = assemblies;
            _assembliesCollection.OnAdd += _onAssemblyAdd;
            _assembliesCollection.OnRemove += _onAssemblyRemove;

            foreach (var assembly in _assembliesCollection)
            {
                _onAssemblyAdd(assembly);
            }
        }

        #region Internal methods for accessing assemblies


        internal IEnumerable<ComponentInfo> GetComponents(AssemblyProvider assembly)
        {
            return _assemblyComponents.Get(assembly);
        }

        internal void RegisterAssembly(string assemblyPath, AssemblyProvider assembly)
        {
            _onAssemblyAdd(assembly);
            var typeAssembly = new TypeAssembly(this, assembly);

            _loadedAssemblies[assemblyPath] = typeAssembly;
        }

        internal TypeAssembly LoadAssembly(string assemblyPath)
        {
            return _loadedAssemblies[assemblyPath];
        }

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
            var calledObjectType = dynamicArgumentInfo[0];
            var methodImplementation = tryDynamicResolve(calledObjectType, method);

            if (methodImplementation == null)
                throw new NotSupportedException("Doesn't have " + calledObjectType + " implementation for: " + method);

            return methodImplementation;
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

            var typePath = new PathInfo(targetTypeName);
            foreach (var assembly in _assemblies)
            {
                var inheritanceChain = assembly.GetInheritanceChain(typePath);

                if (inheritanceChain != null)
                {
                    throw new NotImplementedException();
                }
            }

            throw new NotSupportedException("For type: " + targetTypeName + " there is no inheritance chain");
        }

        internal ComponentInfo GetComponentInfo(InstanceInfo info)
        {
            ComponentInfo result;
            _components.TryGetValue(info, out result);
            return result;
        }

        internal MethodID TryGetImplementation(InstanceInfo type, MethodID abstractMethod)
        {
            return tryDynamicResolve(type, abstractMethod);
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        internal MethodSearcher CreateSearcher()
        {
            return new MethodSearcher(_assembliesCollection);
        }

        internal MethodID GetStaticInitializer(InstanceInfo info)
        {
            return _settings.GetSharedInitializer(info);
        }
        #endregion

        #region Private utility methods

        private MethodID tryDynamicResolve(InstanceInfo dynamicInfo, MethodID method)
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


            foreach (var assembly in _assemblies)
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
            foreach (var assembly in _assemblies)
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

            foreach (var assembly in _assemblies)
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

        private MethodID dynamicExplicitResolve(MethodID method, InstanceInfo dynamicInfo)
        {
            foreach (var assembly in _assemblies)
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

        #endregion

        #region Event handlers

        private void _onAssemblyAdd(AssemblyProvider assembly)
        {
            _assemblies.Add(assembly);
            assembly.TypeServices = _services;
            assembly.OnComponentAdded += (compInfo) => _onComponentAdded(assembly, compInfo);
        }

        private void _onAssemblyRemove(AssemblyProvider assembly)
        {
            assembly.UnloadServices();
        }

        private void _onComponentAdded(AssemblyProvider assembly, ComponentInfo componentInfo)
        {
            _assemblyComponents.Add(assembly, componentInfo);

            if (_components.ContainsKey(componentInfo.ComponentType))
                //TODO how to handle same components
                return;
            _components.Add(componentInfo.ComponentType, componentInfo);
        }

        #endregion
    }
}
