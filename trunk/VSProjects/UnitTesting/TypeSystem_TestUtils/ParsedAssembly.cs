using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

namespace UnitTesting.TypeSystem_TestUtils
{
    class ParsedAssembly:AssemblyProvider
    {
        Dictionary<string, ParsedGenerator> _methods = new Dictionary<string, ParsedGenerator>();
        internal void AddMethod(string name,string source)
        {
            _methods.Add(name, new ParsedGenerator(source));
        }

        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return method.MethodName;
        }

        protected override IInstructionGenerator getGenerator(string methodName)
        {
            return  _methods[methodName];
        }
    }
}
