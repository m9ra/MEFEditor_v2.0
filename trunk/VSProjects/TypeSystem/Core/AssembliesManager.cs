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
            foreach (var assembly in _assemblies)
            {
                var generator = assembly.GetMethodGenerator(method);

                if (generator != null)
                {
                    return generator;
                }
            }

            throw new NotSupportedException("Invalid method: " + method);
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
