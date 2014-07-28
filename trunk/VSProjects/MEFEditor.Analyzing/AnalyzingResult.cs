using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Delegate used for handling view commits. Those handlers
    /// could be used e.g. for writing changes into source instructions.
    /// </summary>
    /// <param name="committedView">The committed view.</param>
    public delegate void OnViewCommit(ExecutionView committedView);

    /// <summary>
    /// Representation of analysis result.
    /// </summary>
    public class AnalyzingResult
    {
        /// <summary>
        /// The instances created during analysis.
        /// </summary>
        private readonly Dictionary<string, Instance> _createdInstances;

        /// <summary>
        /// Method used during analysis.
        /// </summary>
        private readonly HashSet<MethodID> _methods;

        /// <summary>
        /// The entry context of analysis.
        /// </summary>
        public readonly CallContext EntryContext;

        /// <summary>
        /// The return value of entry method.
        /// </summary>
        public readonly Instance ReturnValue;

        /// <summary>
        /// Gets the instances created during analysis.
        /// </summary>
        /// <value>The created instances.</value>
        public IEnumerable<Instance> CreatedInstances { get { return _createdInstances.Values; } }

        /// <summary>
        /// Exception that has been catched during runtime.
        /// </summary>
        /// <value>The runtime exception.</value>
        public Exception RuntimeException { get; internal set; }

        /// <summary>
        /// Occurs when view is committed.
        /// </summary>
        public event OnViewCommit OnViewCommit;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzingResult" /> class.
        /// </summary>
        /// <param name="returnValue">The return value of entry method.</param>
        /// <param name="entryContext">The entry context.</param>
        /// <param name="createdInstances">The created instances.</param>
        /// <param name="methods">The methods.</param>
        internal AnalyzingResult(Instance returnValue, CallContext entryContext, Dictionary<string, Instance> createdInstances, IEnumerable<MethodID> methods)
        {
            ReturnValue = returnValue;
            EntryContext = entryContext;
            _methods = new HashSet<MethodID>(methods);
            _createdInstances = createdInstances;
        }

        /// <summary>
        /// Reports the view commit.
        /// </summary>
        /// <param name="view">The view.</param>
        internal void ReportViewCommit(ExecutionView view)
        {
            if (OnViewCommit != null)
                OnViewCommit(view);
        }

        /// <summary>
        /// Creates the execution view.
        /// </summary>
        /// <returns>ExecutionView.</returns>
        public ExecutionView CreateExecutionView()
        {
            return new ExecutionView(this);
        }

        /// <summary>
        /// Gets the instance according to its ID.
        /// </summary>
        /// <param name="instanceID">The instance identifier.</param>
        /// <returns>Instance.</returns>
        public Instance GetInstance(string instanceID)
        {
            return _createdInstances[instanceID];
        }

        /// <summary>
        /// Determine that given method has been used during interpretation.
        /// </summary>
        /// <param name="method">Tested method.</param>
        /// <returns><c>true</c> if method has been used, <c>false</c> otherwise.</returns>
        public bool Uses(MethodID method)
        {
            return _methods.Contains(method);
        }
    }
}
