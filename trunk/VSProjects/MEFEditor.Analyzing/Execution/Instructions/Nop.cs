using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Nop : InstructionBase
    {
        public override void Execute(AnalyzingContext context)
        {
            //No operation
        }

        public override string ToString()
        {
            return "nop";
        }
    }
}
