using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class DirectInvoke : InstructionBase
    {
        DirectMethod _call;
        public DirectInvoke(DirectMethod call)
        {
            _call = call;
        }

        public override void Execute(AnalyzingContext context)
        {
            _call(context);            
        }

        public override string ToString()
        {
            return string.Format("direct_invoke {0}", _call);
        }
    }
}
