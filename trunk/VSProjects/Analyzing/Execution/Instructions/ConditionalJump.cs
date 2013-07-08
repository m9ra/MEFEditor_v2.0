using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class ConditionalJump<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {        
        private readonly Label _target;
        private readonly VariableName _condition;

        internal ConditionalJump(VariableName condition,Label target)
        {
            _condition = condition;
            _target = target;
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            if (context.IsTrue(_condition))
            {
                context.Jump(_target);
            }
        }
    }
}
