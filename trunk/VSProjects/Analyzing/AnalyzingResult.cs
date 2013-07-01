using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public class AnalyzingResult
    {
        public readonly CallContext EntryContext;

        internal AnalyzingResult(CallContext entryContext)
        {
            EntryContext = entryContext;
        }
    }
}
