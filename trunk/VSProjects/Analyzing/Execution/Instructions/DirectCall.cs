using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class DirectCall : IInstruction
    {
        InstanceFunction _call;
        public DirectCall(InstanceFunction call)
        {
            _call = call;
        }

        public void Execute(AnalyzingContext context)
        {
            var result = _call(context.CurrentArguments);
            context.Return(result);
        }
    }
}
