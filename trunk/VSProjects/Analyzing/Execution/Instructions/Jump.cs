using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Jump<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {        
        private readonly Label _target;

        internal Jump(Label target)
        {
            _target = target;
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            context.Jump(_target);
        }

        public override string ToString()
        {
            return string.Format("jmp {0}", _target);
        }
    }
}
