using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// Class ExecutedBlock.
    /// </summary>
    public class ExecutedBlock
    {
        /// <summary>
        /// Stored scope starts.
        /// </summary>
        private MultiDictionary<Instance, VariableName> _scopeStarts = new MultiDictionary<Instance, VariableName>();

        /// <summary>
        /// Stored scope ends.
        /// </summary>
        private MultiDictionary<Instance, VariableName> _scopeEnds = new MultiDictionary<Instance, VariableName>();

        /// <summary>
        /// Currently available remove providers.
        /// </summary>
        private MultiDictionary<Instance, RemoveTransformProvider> _assignRemoveProviders = new MultiDictionary<Instance, RemoveTransformProvider>();

        /// <summary>
        /// The instances affected in current block.
        /// </summary>
        private HashSet<Instance> _affectedInstances = new HashSet<Instance>();

        /// <summary>
        /// The calls made from current block.
        /// </summary>
        private LinkedList<CallContext> _calls;

        /// <summary>
        /// Next block.
        /// </summary>
        private ExecutedBlock _nextBlock;

        /// <summary>
        /// Information stored to block of instructions.
        /// </summary>
        public readonly InstructionInfo Info;

        /// <summary>
        /// Call where this block has been executed.
        /// </summary>
        public readonly CallContext Call;

        /// <summary>
        /// Gets the previous block.
        /// </summary>
        /// <value>The previous block.</value>
        public ExecutedBlock PreviousBlock { get; private set; }

        /// <summary>
        /// Instances that were affected during execution of current block
        /// </summary>
        /// <value>The affected instances.</value>
        public IEnumerable<Instance> AffectedInstances { get { return _affectedInstances; } }

        /// <summary>
        /// Gets the first block of analysis.
        /// </summary>
        /// <value>The first block.</value>
        public ExecutedBlock FirstBlock
        {
            get
            {
                var call = Call;
                while (call.Caller != null) 
                    call = call.Caller;

                return call.EntryBlock;
            }
        }

        /// <summary>
        /// Gets the next block.
        /// </summary>
        /// <value>The next block.</value>
        public ExecutedBlock NextBlock
        {
            get
            {
                return _nextBlock;
            }
            internal set
            {
                _nextBlock = value;
                _nextBlock.PreviousBlock = this;
            }
        }

        /// <summary>
        /// Determine which variables for given instance has scope start on this block.
        /// </summary>
        /// <param name="instance">Scoped instance.</param>
        /// <returns>Names of variables.</returns>
        public IEnumerable<VariableName> ScopeStarts(Instance instance)
        {
            return _scopeStarts.Get(instance);
        }

        /// <summary>
        /// Determine which variables for given instance has scope end on this block.
        /// </summary>
        /// <param name="instance">Scoped instance.</param>
        /// <returns>Names of variables.</returns>
        public IEnumerable<VariableName> ScopeEnds(Instance instance)
        {
            return _scopeEnds.Get(instance);
        }

        /// <summary>
        /// Get available remove providers for given instance.
        /// </summary>
        /// <param name="instance">The instance which remove providers are requested.</param>
        /// <returns>IEnumerable&lt;RemoveTransformProvider&gt;.</returns>
        public IEnumerable<RemoveTransformProvider> RemoveProviders(Instance instance)
        {
            //remove from assigns
            var assignRemoveProviders = _assignRemoveProviders.Get(instance);            
            foreach (var provider in assignRemoveProviders)
            {
                yield return provider;
            }

            //remove from calls
            foreach (var call in Calls)
            {
                for (int i = 0; i < call.ArgumentValues.Length; ++i)
                {
                    var arg = call.ArgumentValues[i];
                    if (arg == instance)
                    {
                        if (call.TransformProvider.IsOptionalArgument(i))
                        { 
                            //we can remove single argument
                            yield return call.TransformProvider.RemoveArgument(i,false);
                        }
                        else
                        {
                            //remove whole call
                            yield return call.TransformProvider.Remove();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets calls invoked from current block.
        /// </summary>
        /// <value>The calls.</value>
        public IEnumerable<CallContext> Calls
        {
            get
            {
                if (_calls == null)
                {
                    return new CallContext[0];
                }
                else
                {
                    return _calls;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedBlock" /> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="call">The call.</param>
        internal ExecutedBlock(InstructionInfo info, CallContext call)
        {
            Info = info;
            Call = call;
        }

        /// <summary>
        /// Register call called from current block.
        /// </summary>
        /// <param name="callContext">Context of the call.</param>
        internal void RegisterCall(CallContext callContext)
        {
            if (_calls == null)
            {
                _calls = new LinkedList<CallContext>();
            }
            _calls.AddLast(callContext);
        }

        /// <summary>
        /// Registers the assign processed by given instruction..
        /// </summary>
        /// <param name="scopedVariable">The scoped variable.</param>
        /// <param name="assignInstruction">The assign instruction.</param>
        /// <param name="oldInstance">The old instance.</param>
        /// <param name="assignedInstance">The assigned instance.</param>
        internal void RegisterAssign(VariableName scopedVariable, AssignBase assignInstruction, Instance oldInstance, Instance assignedInstance)
        {
            if (scopedVariable.Name.StartsWith("$"))
            {
                //TODO: refactor temporary variables
                //dont save temporary variables
                return;
            }

            if (oldInstance != null)
            {
                //there was another instance in variable - its scope is ending
                _scopeEnds.Add(oldInstance, scopedVariable);
                _affectedInstances.Add(oldInstance);
            }

            _affectedInstances.Add(assignedInstance);
            _scopeStarts.Add(assignedInstance, scopedVariable);

            RemoveTransformProvider removeProvider = null;
            if (assignInstruction != null && assignInstruction.RemoveProvider != null)
            {
                removeProvider = assignInstruction.RemoveProvider;
            }

            _assignRemoveProviders.Add(assignedInstance, removeProvider);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (Info.Comment == null)
                return base.ToString();

            return "[ExecutedBlock]" + Info.Comment;
        }
    }
}
