using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{

    /// <summary>
    /// Prepares arguments for call invoking
    /// </summary>
    class PreCall : InstructionBase
    {
        readonly VariableName[] _arguments;
        public PreCall(IEnumerable<VariableName> arguments)
        {
            _arguments = arguments.ToArray();
        }
        public override void Execute(AnalyzingContext context)
        {
            context.PrepareCall(_arguments);
        }

        public override string ToString()
        {
            return string.Format("prepare_call {0}", string.Join(", ",_arguments.Skip(0)));
        }
    }
}
