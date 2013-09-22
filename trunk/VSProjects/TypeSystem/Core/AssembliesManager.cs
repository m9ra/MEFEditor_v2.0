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
            var result = staticExplicitResolve(method);

            if (result == null)
            {
                result = staticGenericResolve(method);
            }

            if (result == null)
                throw new NotSupportedException("Invalid method: " + method);

            return result;
        }

        /// <summary>
        /// Resolve method generator with generic search on given method ID
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <returns>Generator for resolved method, or null, if there is no available generator</returns>
        private GeneratorBase staticGenericResolve(MethodID method) {
            //test if method is generic
            string path, paramDescr;
            Naming.GetParts(method, out path, out paramDescr);

            var searchPath = new PathInfo(path);
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

        internal ComponentInfo GetComponentInfo(Instance instance)
        {
            ComponentInfo result;
            _components.TryGetValue(instance.Info, out result);
            return result;
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

    }
}
