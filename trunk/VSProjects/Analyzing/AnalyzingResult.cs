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
        private readonly RemoveHandler _removeHandler;

        private readonly Dictionary<string, Instance> _createdInstances;

        public readonly CallContext EntryContext;

        public IEnumerable<Instance> CreatedInstances { get { return _createdInstances.Values; } }

        public event Action OnTransformationCommit;

        internal AnalyzingResult(CallContext entryContext, RemoveHandler removeHandler, Dictionary<string, Instance> createdInstances)
        {
            EntryContext = entryContext;
            _removeHandler = removeHandler;
            _createdInstances = createdInstances;
        }

        internal void ReportTransformationCommit()
        {
            if (OnTransformationCommit != null)
                OnTransformationCommit();
        }

        public TransformationServices CreateTransformationServices()
        {
            return new TransformationServices(this, _removeHandler);
        }

        public Instance GetInstance(string instanceID)
        {
            return _createdInstances[instanceID];
        }
    }
}
