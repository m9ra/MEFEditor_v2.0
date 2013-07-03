using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    interface IInstruction<MethodID,InstanceInfo>
    {
        void Execute(AnalyzingContext<MethodID,InstanceInfo> context);
    }
}
