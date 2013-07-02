using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class DirectCall : IInstruction
    {
        DirectMethod _call;
        public DirectCall(DirectMethod call)
        {
            _call = call;
        }

        public void Execute(AnalyzingContext context)
        {
            _call(context);            
        }
    }
}
