using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    public class AssemblyLoader : LoaderBase
    {
        readonly AssembliesManager _assemblies;
        

        public AssemblyLoader(AssemblyCollection assemblies)
        {
            _assemblies = new AssembliesManager(assemblies);
        }

        public override GeneratorBase EntryPoint
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
    }
}
