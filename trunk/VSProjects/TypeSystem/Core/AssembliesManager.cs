using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Utilities;
using Analyzing;


namespace TypeSystem.Core
{
    class AssembliesManager
    {
        /// <summary>
        /// In these assemblies are searched generators
        /// <remarks>May differ from method searcher providing assemblies - that are based on assembly references</remarks>
        /// </summary>
        readonly Dictionary<AssemblyProvider, TypeAssembly> _assemblies = new Dictionary<AssemblyProvider, TypeAssembly>();

        /// <summary>
        /// Assembly providers indexed by their full paths
        /// </summary>
        readonly Dictionary<string, AssemblyProvider> _assemblyPathIndex = new Dictionary<string, AssemblyProvider>();

        readonly TypeServices _services;

        readonly MultiDictionary<AssemblyProvider, ComponentInfo> _assemblyComponents = new MultiDictionary<AssemblyProvider, ComponentInfo>();

        readonly Dictionary<InstanceInfo, ComponentInfo> _components = new Dictionary<InstanceInfo, ComponentInfo>();

        internal readonly AssemblyCollectionBase Assemblies;

        internal readonly MachineSettings Settings;

        internal event ComponentEvent ComponentAdded;

        internal event ComponentEvent ComponentRemoved;

        internal IEnumerable<ComponentInfo> Components { get { return _components.Values; } }

        internal AssembliesManager(AssemblyCollectionBase assemblies, MachineSettings settings)
        {
            _services = new TypeServices(this);
            Settings = settings;

            Assemblies = assemblies;
            Assemblies.OnAdd += _onAssemblyAdd;
            Assemblies.OnRemove += _onAssemblyRemove;

            foreach (var assembly in Assemblies)
            {
                _onAssemblyAdd(assembly);
            }
        }

        #region Internal methods for accessing assemblies


        internal IEnumerable<ComponentInfo> GetComponents(AssemblyProvider assembly)
        {
            return _assemblyComponents.Get(assembly);
        }

        internal void RegisterAssembly(AssemblyProvider assembly)
        {
            _onAssemblyAdd(assembly);
        }

        internal TypeAssembly LoadAssembly(string assemblyPath)
        {
            //TODO performance improvement
            //TODO loading of not registered assemblies (because of directory catalogs)

            foreach (var assemblyPair in _assemblies)
            {
                if (assemblyPair.Key.FullPathMapping == assemblyPath)
                    return assemblyPair.Value;
            }

            return null;
        }

        internal TypeAssembly DefiningAssembly(MethodID callerId)
        {
            foreach (var assemblyPair in _assemblies)
            {
                var assembly = assemblyPair.Key;
                var generator = assembly.GetMethodGenerator(callerId);
                if (generator != null)
                    return assemblyPair.Value;
            }

            return null;
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
            var calledObjectType = dynamicArgumentInfo[0] as TypeDescriptor;
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

            var chain = getChain(assignedTypeName);
            return chain.HasSubChain(targetTypeName);
        }

        internal ComponentInfo GetComponentInfo(InstanceInfo info)
        {
            ComponentInfo result;
            _components.TryGetValue(info, out result);
            return result;
        }

        internal MethodID TryGetImplementation(TypeDescriptor type, MethodID abstractMethod)
        {
            return tryDynamicResolve(type, abstractMethod);
        }

        /// <summary>
        /// Creates method searcher, which can search in referenced assemblies
        /// </summary>
        /// <returns>Created method searcher</returns>
        internal MethodSearcher CreateSearcher()
        {
            return new MethodSearcher(Assemblies);
        }

        internal MethodID GetStaticInitializer(InstanceInfo info)
        {
            return Settings.GetSharedInitializer(info);
        }

        internal IEnumerable<string> GetFiles(string directoryFullPath)
        {
            var realFiles = Directory.GetFiles(directoryFullPath);
            foreach (var realFile in realFiles)
            {
                if (_assemblyPathIndex.ContainsKey(realFile))
                    //assemblies are added according to their mapping
                    continue;

                yield return realFile;
            }


            foreach (var assembly in _assemblies.Keys)
            {
                if (!assembly.FullPathMapping.StartsWith(directoryFullPath))
                    //directory has to match begining of path
                    continue;

                var mappedDirectory = Path.GetDirectoryName(assembly.FullPathMapping);
                if (mappedDirectory == directoryFullPath)
                    yield return assembly.FullPathMapping;
            }
        }

        internal InheritanceChain CreateChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            //TODO caching

            return new InheritanceChain(type, subChains);
        }

        internal InheritanceChain GetChain(TypeDescriptor type)
        {
            return getChain(type.TypeName);
        }

        private InheritanceChain getChain(string typeName)
        {
            var typePath = new PathInfo(typeName);
            foreach (var assembly in _assemblies.Keys)
            {
                var inheritanceChain = assembly.GetInheritanceChain(typePath);

                if (inheritanceChain != null)
                {
                    return inheritanceChain;
                }
            }

            throw new NotSupportedException("For type: " + typeName + " there is no inheritance chain");
        }

        #endregion

        #region Private utility methods

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


            foreach (var assembly in _assemblies.Keys)
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
            foreach (var assembly in _assemblies.Keys)
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

            foreach (var assembly in _assemblies.Keys)
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
            foreach (var assembly in _assemblies.Keys)
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
            _assemblyPathIndex[assembly.FullPath] = assembly;

            var typeAssembly = new TypeAssembly(this, assembly);
            _assemblies.Add(assembly, typeAssembly);
            assembly.ComponentAdded += (compInfo) => _onComponentAdded(assembly, compInfo);
            assembly.TypeServices = _services;
        }

        private void _onAssemblyRemove(AssemblyProvider assembly)
        {
            var componentsCopy = GetComponents(assembly).ToArray();
            foreach (var component in componentsCopy)
            {
                _onComponentRemoved(assembly, component);
            }

            _assemblyPathIndex.Remove(assembly.FullPath);
            _assemblies.Remove(assembly);
            assembly.UnloadServices();
        }

        private void _onComponentAdded(AssemblyProvider assembly, ComponentInfo componentInfo)
        {
            componentInfo.DefiningAssembly = _assemblies[assembly];

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

    }
}
