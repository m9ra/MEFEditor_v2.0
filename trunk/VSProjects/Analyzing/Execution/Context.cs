using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class Context
    {
        public bool ExecutionEnd { get; private set; }

        internal Instance GetValue(VariableName _sourceVariable)
        {
            throw new NotImplementedException();
        }
        internal void SetValue(VariableName _targetVariable, Instance value)
        {
            throw new NotImplementedException();
        }

        internal void FetchInstructions(IInstructionGenerator generator)
        {
            throw new NotImplementedException();
        }

        internal void PrepareArguments(VariableName[] _arguments)
        {
            throw new NotImplementedException();
        }

        internal IInstruction NextInstruction()
        {
            throw new NotImplementedException();
        }

        internal IInstructionGenerator GetGenerator(VersionedName _methodGeneratorName)
        {
            throw new NotImplementedException();
        }

        internal AnalyzingResult GetResult()
        {
            throw new NotImplementedException();
        }
    }
}
