using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class DirectInvoke<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {
        DirectMethod<MethodID, InstanceInfo> _call;
        public DirectInvoke(DirectMethod<MethodID, InstanceInfo> call)
        {
            _call = call;
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            _call(context);            
        }

        public override string ToString()
        {
            return string.Format("direct_invoke {0}", _call);
        }
    }
}
