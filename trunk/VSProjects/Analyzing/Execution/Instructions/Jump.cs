using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Jump<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {        
        internal readonly Label Target;

        internal Jump(Label target)
        {
            Target = target;
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            context.Jump(Target);
        }

        public override string ToString()
        {
            return string.Format("jmp {0}", Target);
        }
    }
}
