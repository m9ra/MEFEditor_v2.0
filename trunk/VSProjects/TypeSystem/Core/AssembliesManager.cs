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

        private void onAssemblyAdd(AssemblyProvider assembly)
        {
            assembly.SetServices(_services);
        }

        private void onAssemblyRemove(AssemblyProvider assembly)
        {
            assembly.UnloadServices();
        }

        internal MethodSearcher CreateSearcher()
        {
            return new MethodSearcher(_assemblies);
        }
    }
}
