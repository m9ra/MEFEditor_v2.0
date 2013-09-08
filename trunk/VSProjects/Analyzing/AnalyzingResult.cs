using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution;

namespace Analyzing
{
    public class AnalyzingResult
    {
        public readonly CallContext EntryContext;
        private readonly RemoveHandler _removeHandler;
        
        internal AnalyzingResult(CallContext entryContext,RemoveHandler removeHandler)
        {
            EntryContext = entryContext;
            _removeHandler = removeHandler;
        }

        public TransformationServices CreateTransformationServices()
        {
            return new TransformationServices(_removeHandler);
        }
    }
}
