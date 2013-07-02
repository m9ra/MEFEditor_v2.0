using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing;
using Analyzing.Execution;

namespace UnitTesting.Analyzing_TestUtils
{

    delegate void EmitDirector(IEmitter emitter);

    static class ExecutionUtils
    {
        public static AnalyzingResult Run(EmitDirector director)
        {
            var machine = new Machine(Environment.SettingsProvider.MachineSettings);
            var loader=new TestLoader(director);
            return machine.Run(loader);
        }

        internal static TestCase AssertVariable(this AnalyzingResult result, string variable)
        {
            return new TestCase(result, variable);
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
