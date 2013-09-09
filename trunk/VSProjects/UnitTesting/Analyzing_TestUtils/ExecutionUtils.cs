using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing;
using Analyzing.Execution;

using TypeSystem;

using UnitTesting.TypeSystem_TestUtils;
using UnitTesting.Analyzing_TestUtils.Environment;

namespace UnitTesting.Analyzing_TestUtils
{

    delegate void EmitDirector(EmitterBase emitter);

    public static class ExecutionUtils
    {
        internal static AnalyzingResult Run(EmitDirector director)
        {
            var machine = new Machine(SettingsProvider.MachineSettings);

            var assembly=SettingsProvider.CreateTestingAssembly();
            assembly.Runtime.BuildAssembly();
            var loader = new EmitDirectorLoader(director,assembly.Loader);
            return machine.Run(loader);
        }

        internal static TestCase AssertVariable(this AnalyzingResult result, string variable)
        {
            return new TestCase(result, variable);
        }
        
        public static IEnumerable<CallContext> ChildContexts(this CallContext callContext)
        {
            var block = callContext.EntryBlock;
            while (block != null)
            {
                foreach (var childContext in block.Calls)
                {
                    yield return childContext;
                }
                block = block.NextBlock;
            }
        }
    }

    internal class TestCase
    {
        private readonly AnalyzingResult _result;
        private readonly VariableName _variable;

        internal TestCase(AnalyzingResult result, string variable)
        {            
            _result = result;
            _variable = new VariableName(variable);
        }
        internal TestCase AssertVariable(string variable)
        {
            return new TestCase(_result, variable);
        }

        internal TestCase HasValue(object expectedValue)
        {
            var instance = _result.EntryContext.GetValue(_variable);
            var actualValue = instance.DirectValue;

            Assert.AreEqual(expectedValue, actualValue);
            return this;
        }
    }
}
