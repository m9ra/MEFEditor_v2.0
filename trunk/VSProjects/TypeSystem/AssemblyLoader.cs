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

        public override VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            var thisInfo = staticArgumentInfo[0];

            VersionedName callName;
            if (_assemblies.TryResolveMethod(method, staticArgumentInfo, out callName))
            {
                return callName;
            }

            throw new NotImplementedException("method not found");
        }

        public override GeneratorBase GetGenerator(VersionedName methodName)
        {
            return _assemblies.GetGenerator(methodName);
        }

        public override VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
