using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using Analyzing;

using UnitTesting.Analyzing_TestUtils;

namespace UnitTesting.TypeSystem_TestUtils
{
    static class AssemblyUtils
    {
        public static readonly string EntryMethodName="EntryMethod";

        internal static ParsedAssembly Run(string entryMethod)
        {
            var assembly = new ParsedAssembly();
            assembly.AddMethod(EntryMethodName, entryMethod);

            return assembly;
        }

        internal static TestCase AssertVariable(this ParsedAssembly assembly, string variableName)
        {
            var testAssemblies = new TestAssemblyCollection(assembly);
            var loader = new AssemblyLoader(testAssemblies);
            var entryLoader = new EntryPointLoader(
                new VersionedName(EntryMethodName, 0)
                , loader);


            var machine = new Machine<MethodID, InstanceInfo>(new MachineSettings());
            var result = machine.Run(entryLoader);

            return new TestCase(result, variableName);
        }
        
    }
}
