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

        public readonly IEnumerable<Instance> CreatedInstances;

        internal AnalyzingResult(CallContext entryContext,RemoveHandler removeHandler, IEnumerable<Instance> createdInstances)
        {
            EntryContext = entryContext;
            _removeHandler = removeHandler;
            CreatedInstances = createdInstances;
        }

        public TransformationServices CreateTransformationServices()
        {
            return new TransformationServices(_removeHandler);
        }
    }
}
