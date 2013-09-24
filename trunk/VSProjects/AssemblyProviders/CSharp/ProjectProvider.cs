using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;


namespace AssemblyProviders.CSharp
{
    public class ProjectProvider:AssemblyProvider
    {        
        public override SearchIterator CreateRootIterator()
        {
            throw new NotImplementedException();
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            throw new NotImplementedException();
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            throw new NotImplementedException();
        }

        public override MethodID GetImplementation(MethodID method, InstanceInfo dynamicInfo)
        {
            throw new NotImplementedException();
        }

        public override MethodID GetGenericImplementation(MethodID method, PathInfo searchPath, InstanceInfo dynamicInfo)
        {
            throw new NotImplementedException();
        }
    }
}
