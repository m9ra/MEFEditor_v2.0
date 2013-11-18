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
        internal static TestResult Run(EmitDirector director)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            var machine = SettingsProvider.CreateMachine(assembly.Settings);

            assembly.Runtime.BuildAssembly();
            var loader = new EmitDirectorLoader(director, assembly.Loader);
            return new TestResult(machine.Run(loader));
        }

        internal static TestCase AssertVariable(this TestResult result, string variable)
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
        private readonly TestResult _result;
        private readonly VariableName _variable;

        internal TestCase(TestResult result, string variable)
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
            var instance = _result.Execution.EntryContext.GetValue(_variable);
            var actualValue = instance.DirectValue;

            Assert.AreEqual(expectedValue, actualValue);
            return this;
        }
    }
}
