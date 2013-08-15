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
    public static class AssemblyUtils
    {
        public static readonly string EntryMethodName="EntryMethod";

        public static ParsedAssembly Run(string entryMethod)
        {
            var assembly = new ParsedAssembly();
            assembly.AddMethod(EntryMethodName, entryMethod);

            return assembly;
        }

        public static Analyzing.AnalyzingResult<MethodID,InstanceInfo> GetResult(this ParsedAssembly assembly)
        {
            var testAssemblies = new TestAssemblyCollection(assembly);
            var loader = new AssemblyLoader(testAssemblies);
            var entryLoader = new EntryPointLoader(
                new VersionedName(EntryMethodName, 0)
                , loader);


            var machine = new Machine<MethodID, InstanceInfo>(new MachineSettings());
            return machine.Run(entryLoader);
        }

        internal static TestCase AssertVariable(this ParsedAssembly assembly, string variableName)
        {
            var result = assembly.GetResult();
            
            return new TestCase(result, variableName);
        }
        
    }
}
