using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution;

namespace Analyzing
{
    public class AnalyzingResult<MethodID, InstanceInfo>
    {
        public readonly CallContext<MethodID, InstanceInfo> EntryContext;
        public readonly TransformationServices TransformationServices;

        internal AnalyzingResult(CallContext<MethodID, InstanceInfo> entryContext)
        {
            EntryContext = entryContext;
            TransformationServices = new TransformationServices();
        }
    }
}
