using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class ConditionalJump : InstructionBase
    {        
        internal readonly Label Target;
        private readonly VariableName _condition;

        internal ConditionalJump(VariableName condition,Label target)
        {
            _condition = condition;
            Target = target;
        }

        public override void Execute(AnalyzingContext context)
        {
            if (context.IsTrue(_condition))
            {
                context.Jump(Target);
            }
        }

        public override string ToString()
        {
            return string.Format("jmp {0} if {1}", Target, _condition);
        }
    }
}
