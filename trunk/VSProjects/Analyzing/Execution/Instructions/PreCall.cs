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
    class PreCall<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {
        readonly VariableName[] _arguments;
        public PreCall(IEnumerable<VariableName> arguments)
        {
            _arguments = arguments.ToArray();
        }
        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            context.PrepareCall(_arguments);
        }
    }
}
