using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;

using Analyzing.Editing;
using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    public class ExecutedBlock<MethodID, InstanceInfo>
    {
        private MultiDictionary<Instance, VariableName> _scopeStarts = new MultiDictionary<Instance, VariableName>();
        private MultiDictionary<Instance, VariableName> _scopeEnds = new MultiDictionary<Instance, VariableName>();
        private MultiDictionary<Instance, RemoveTransformProvider> _removeProviders = new MultiDictionary<Instance, RemoveTransformProvider>();

        private HashSet<Instance> _affectedInstances = new HashSet<Instance>();

        private LinkedList<CallContext<MethodID, InstanceInfo>> _calls;

        private ExecutedBlock<MethodID, InstanceInfo> _nextBlock;

        public readonly InstructionInfo Info;

        /// <summary>
        /// Call where this block has been executed
        /// </summary>
        public readonly CallContext<MethodID, InstanceInfo> Call;

        public ExecutedBlock<MethodID, InstanceInfo> PreviousBlock { get; private set; }

        /// <summary>
        /// Instances that were affected during execution of current block
        /// TODO: Other than assigns affecting, deep calls affecting
        /// </summary>
        public IEnumerable<Instance> AffectedInstances { get { return _affectedInstances; } }

        public ExecutedBlock<MethodID, InstanceInfo> NextBlock
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
        /// Determine which variables for given instance has scope start on this block
        /// </summary>
        /// <param name="instance">Scoped instance</param>
        /// <returns>Names of variables</returns>
        public IEnumerable<VariableName> ScopeStarts(Instance instance)
        {
            return _scopeStarts.GetValues(instance);
        }

        /// <summary>
        /// Determine which variables for given instance has scope end on this block
        /// </summary>
        /// <param name="instance">Scoped instance</param>
        /// <returns>Names of variables</returns>
        public IEnumerable<VariableName> ScopeEnds(Instance instance)
        {
            return _scopeEnds.GetValues(instance);
        }

        public IEnumerable<RemoveTransformProvider> RemoveProviders(Instance instance)
        {
            foreach (var provider in _removeProviders.GetValues(instance))
            {
                yield return provider;
            }

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
                            yield return call.TransformProvider.RemoveArgument(i);
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


        public IEnumerable<CallContext<MethodID, InstanceInfo>> Calls
        {
            get
            {
                if (_calls == null)
                {
                    return new CallContext<MethodID, InstanceInfo>[0];
                }
                else
                {
                    return _calls;
                }
            }
        }

        internal ExecutedBlock(InstructionInfo info, CallContext<MethodID, InstanceInfo> call)
        {
            Info = info;
            Call = call;
        }

        internal void RegisterCall(CallContext<MethodID, InstanceInfo> callContext)
        {
            if (_calls == null)
            {
                _calls = new LinkedList<CallContext<MethodID, InstanceInfo>>();
            }
            _calls.AddLast(callContext);
        }

        internal void RegisterAssign(VariableName scopedVariable, AssignBase<MethodID, InstanceInfo> assignInstruction, Instance oldInstance, Instance assignedInstance)
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

            _removeProviders.Add(assignedInstance, removeProvider);
        }
    }
}
