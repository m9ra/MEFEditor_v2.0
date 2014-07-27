using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    class Jump : InstructionBase
    {        
        internal readonly Label Target;

        internal Jump(Label target)
        {
            Target = target;
        }

        public override void Execute(AnalyzingContext context)
        {
            context.Jump(Target);
        }

        public override string ToString()
        {
            return string.Format("jmp {0}", Target);
        }
    }
}
