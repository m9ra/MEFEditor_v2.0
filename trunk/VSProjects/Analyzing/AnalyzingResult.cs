using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public class AnalyzingResult<MethodID, InstanceInfo>
    {
        public readonly CallContext<MethodID, InstanceInfo> EntryContext;

        internal AnalyzingResult(CallContext<MethodID, InstanceInfo> entryContext)
        {
            EntryContext = entryContext;
        }
    }
}
