using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;




namespace AssemblyProviders.CSharp
{
    public class ProjectProvider:AssemblyProvider
    {
        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            throw new NotImplementedException();
        }


        protected override IInstructionGenerator getGenerator(string methodName)
        {
            throw new NotImplementedException();
        }
    }
}
