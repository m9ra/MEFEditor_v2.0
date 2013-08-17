using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Nop<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {
        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            //No operation
        }

        public override string ToString()
        {
            return "nop";
        }
    }
}
