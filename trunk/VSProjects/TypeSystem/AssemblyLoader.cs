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
        readonly AssembliesManager _manager;

        public readonly AppDomainServices AppDomain;

        public MachineSettings Settings { get { return _manager.Settings; } }

        public AssemblyLoader(AssemblyCollectionBase assemblies, MachineSettings settings)
        {
            _manager = new AssembliesManager(assemblies, settings);
            AppDomain = new AppDomainServices(_manager);
        }
        
        public override GeneratorBase StaticResolve(MethodID method)
        {
            return _manager.StaticResolve(method);
        }

        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _manager.DynamicResolve(method, dynamicArgumentInfo);
        }

        public ComponentInfo GetComponentInfo(InstanceInfo instanceInfo)
        {
            return _manager.GetComponentInfo(instanceInfo);
        }
    }
}
