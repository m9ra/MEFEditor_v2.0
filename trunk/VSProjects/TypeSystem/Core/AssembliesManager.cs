using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem.Core
{
    class AssembliesManager
    {
        readonly AssemblyCollection _assemblies;

        readonly TypeServices _services;

        readonly Dictionary<InstanceInfo, ComponentInfo> _components = new Dictionary<InstanceInfo, ComponentInfo>();

        internal AssembliesManager(AssemblyCollection assemblies)
        {
            _services = new TypeServices(this);

            _assemblies = assemblies;
            _assemblies.OnAdd += onAssemblyAdd;
            _assemblies.OnRemove += onAssemblyRemove;

            foreach (var assembly in _assemblies)
            {
                onAssemblyAdd(assembly);
            }
        }

        internal GeneratorBase StaticResolve(MethodID method)
        {
            var result = tryStaticResolve(method);

            if (result == null)
                throw new NotSupportedException("Invalid method: " + method);

            return result;
        }

        internal GeneratorBase DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            //resolving of .NET objects depends only on this object type
            var methodImplementation = tryDynamicResolve(dynamicArgumentInfo[0], method);

            return StaticResolve(methodImplementation);
        }

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
            var searchPath = getSearchPath(method);
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
            var searchPath = getSearchPath(method);
            if (!searchPath.HasGenericArguments)
                //there is no need for generic resolving
                return null;

            foreach (var assembly in _assemblies)
            {
                var implementation = assembly.GetGenericImplementation(method, searchPath, dynamicInfo);
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

        internal ComponentInfo GetComponentInfo(Instance instance)
        {
            ComponentInfo result;
            _components.TryGetValue(instance.Info, out result);
            return result;
        }

        internal MethodID TryGetImplementation(InstanceInfo type, MethodID abstractMethod)
        {
            return tryDynamicResolve(type, abstractMethod);
        }

        internal MethodSearcher CreateSearcher()
        {
            return new MethodSearcher(_assemblies);
        }

        private void onAssemblyAdd(AssemblyProvider assembly)
        {
            assembly.SetServices(_services);
            assembly.OnComponentAdded += _onComponentAdded;
        }

        private void onAssemblyRemove(AssemblyProvider assembly)
        {
            assembly.UnloadServices();
        }

        private void _onComponentAdded(InstanceInfo instanceInfo, ComponentInfo componentInfo)
        {
            _components.Add(instanceInfo, componentInfo);
        }

        private static PathInfo getSearchPath(MethodID method)
        {
            //test if method is generic
            string path, paramDescr;
            Naming.GetParts(method, out path, out paramDescr);

            return new PathInfo(path);
        }

    }
}
