using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing
{
    public delegate void OnViewCommit(ExecutionView commitedView);

    public class AnalyzingResult
    {
        private readonly Dictionary<string, Instance> _createdInstances;

        private readonly HashSet<MethodID> _methods;

        public readonly CallContext EntryContext;

        public readonly Instance ReturnValue;

        public IEnumerable<Instance> CreatedInstances { get { return _createdInstances.Values; } }

        /// <summary>
        /// Exception that has been catched during runtime
        /// </summary>
        public Exception RuntimeException { get; internal set; }

        public event OnViewCommit OnViewCommit;



        internal AnalyzingResult(Instance returnValue, CallContext entryContext, Dictionary<string, Instance> createdInstances, IEnumerable<MethodID> methods)
        {
            ReturnValue = returnValue;
            EntryContext = entryContext;
            _methods = new HashSet<MethodID>(methods);
            _createdInstances = createdInstances;
        }

        internal void ReportViewCommit(ExecutionView view)
        {
            if (OnViewCommit != null)
                OnViewCommit(view);
        }

        public ExecutionView CreateExecutionView()
        {
            return new ExecutionView(this);
        }

        public Instance GetInstance(string instanceID)
        {
            return _createdInstances[instanceID];
        }

        /// <summary>
        /// Determine that given method has been used during interpretation
        /// </summary>
        /// <param name="method">Tested method</param>
        /// <returns><c>true</c> if method has been used, <c>false</c> otherwise</returns>
        public bool Uses(MethodID method)
        {
            return _methods.Contains(method);
        }
    }
}
