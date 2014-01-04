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
        readonly AssembliesManager _assemblies;

        public AssemblyLoader(AssemblyCollection assemblies, MachineSettings settings)
        {
            _assemblies = new AssembliesManager(assemblies, settings);
        }

        public override MethodID EntryPoint
        {
            get { throw new NotImplementedException(); }
        }

        public override GeneratorBase StaticResolve(MethodID method)
        {
            return _assemblies.StaticResolve(method);
        }

        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _assemblies.DynamicResolve(method, dynamicArgumentInfo);
        }

        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _assemblies.GetComponentInfo(instanceInfo);
        }
    }
}
