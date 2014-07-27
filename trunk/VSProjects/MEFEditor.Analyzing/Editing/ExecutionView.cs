using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing
{
    public delegate T ViewDataProvider<T>()
        where T : ExecutionViewData;

    /// <summary>
    /// Is available when transformation is applied to provide transformation services
    /// </summary>
    public class ExecutionView
    {
        public string AbortMessage { get; private set; }

        public bool IsAborted { get { return AbortMessage != null; } }

        public bool IsCommited { get; private set; }

        private readonly InstanceRemoveProvider _instanceRemoveProvider;

        private readonly AnalyzingResult _result;

        private readonly ExecutionViewDataHandler _viewData = new ExecutionViewDataHandler();

        private readonly Dictionary<ExecutedBlock, ExecutedBlock> _nextChanges = new Dictionary<ExecutedBlock, ExecutedBlock>();

        private readonly Dictionary<ExecutedBlock, ExecutedBlock> _previouChanges = new Dictionary<ExecutedBlock, ExecutedBlock>();

        private List<Transformation> _appliedTransformations = new List<Transformation>();

        public ExecutedBlock EntryBlock { get { return _result.EntryContext.EntryBlock; } }

        internal ExecutionView(AnalyzingResult result)
        {
            _result = result;
            _instanceRemoveProvider = new InstanceRemoveProvider(result.EntryContext);
        }

        /// <summary>
        /// Abort current view with given message
        /// </summary>
        /// <param name="abortMessage">Message with description why view is aborted</param>
        /// <returns>Null - because of shorter notation possibility</returns>
        public object Abort(string abortMessage)
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot abort twice");
            }
            AbortMessage = abortMessage;

            return null;
        }

        /// <summary>
        /// Apply transformation on current view
        /// </summary>
        /// <param name="transformation">Transformation to be applied</param>
        public void Apply(Transformation transformation)
        {
            if (!IsAborted)
            {
                _appliedTransformations.Add(transformation);
                transformation.Apply(this);
            }
        }

        /// <summary>
        /// Commit all contained view data
        /// </summary>
        public void Commit()
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot commit when aborted");
            }

            if (IsCommited)
            {
                throw new NotSupportedException("Cannot commit view twice");
            }

            _viewData.Commit();

            _appliedTransformations = null;

            _result.ReportViewCommit(this);
        }


        /// <summary>
        /// Clone current view into view that is independent on current view.
        /// <remarks>Only one view in views clonning hirarchy can be commited</remarks>
        /// </summary>
        /// <returns>Cloned view</returns>
        public ExecutionView Clone()
        {
            if (IsAborted)
                throw new NotSupportedException("Cannot clone aborted view");

            if (IsCommited)
                throw new NotSupportedException("Cannot clone commited view");

            var clone = new ExecutionView(_result);
            clone._appliedTransformations.AddRange(_appliedTransformations);
            clone._viewData.FillFrom(_viewData);

            return clone;
        }


        /// <summary>
        /// Get data stored in current view for given key and type.
        /// If no matching data are found, new data is created via provider.
        /// </summary>
        /// <typeparam name="T">Type of searched data</typeparam>
        /// <param name="key">Key of data - because multiple sources can store data with same type</param>
        /// <param name="provider">Provider used for data creation</param>
        /// <returns>Stored data, created data, or null if the data is not found and provider is not present</returns>
        public T Data<T>(object key, ViewDataProvider<T> provider = null)
            where T : ExecutionViewData
        {
            return _viewData.Data<T>(key, provider);
        }

        #region View Transformation API

        public bool Remove(Instance instance)
        {
            return _instanceRemoveProvider.Remove(instance, this);
        }

        public bool CanRemove(Instance instance)
        {
            return _instanceRemoveProvider.CanRemove(instance, this);
        }

        public ExecutedBlock NextBlock(ExecutedBlock block)
        {
            return fromChanges(_nextChanges, block, block.NextBlock);
        }

        public ExecutedBlock PreviousBlock(ExecutedBlock block)
        {
            return fromChanges(_previouChanges, block, block.PreviousBlock);
        }

        public IEnumerable<Instance> AffectedInstances(ExecutedBlock block)
        {
            return block.AffectedInstances;
        }

        internal IEnumerable<VariableName> ScopeStarts(ExecutedBlock block, Instance instance)
        {
            return block.ScopeStarts(instance);
        }

        internal IEnumerable<VariableName> ScopeEnds(ExecutedBlock block, Instance instance)
        {
            return block.ScopeEnds(instance);
        }

        public ExecutedBlock LastCallBlock(Instance instance)
        {
            var current = instance.CreationBlock;

            ExecutedBlock lastCall = null;
            while (current != null)
            {
                foreach (var call in current.Calls)
                {
                    if (call.ArgumentValues.Length == 0)
                        continue;

                    if (call.ArgumentValues[0] == instance)
                    {
                        //we have found block where instanc is "called"
                        lastCall = current;
                        //break outer foreach, because it is
                        //not needed to search for next calls in block
                        break;
                    }
                }
                current = NextBlock(current);
            }

            return lastCall;
        }

        /// <summary>
        /// Determine if block1 is in current view before block2
        /// </summary>
        /// <param name="block1"></param>
        /// <param name="block2"></param>
        /// <returns></returns>
        public bool IsBefore(ExecutedBlock block1, ExecutedBlock block2)
        {
            if (block1 == null || block2==null)
                //null is negative infinity in block1
                return true;

            var commonCall = GetCommonCall(block1, block2);

            var block1_level = GetBlockInLevelOf(commonCall, block1);
            var block2_level = GetBlockInLevelOf(commonCall, block2);

            var current = block2_level;
            while (current != null)
            {
                if (current == block1_level)
                    //block1 is before block2
                    return true;

                //step backword
                current = PreviousBlock(current);
            }
            //we didnt meet block1 when stepping backward from block2
            return false;
        }


        /// <summary>
        /// Get last of given blocks in current view ordering
        /// </summary>
        /// <param name="blocks">Blocks where last one is searched</param>
        /// <returns>Last of given blocks</returns>
        public ExecutedBlock LatestBlock(IEnumerable<ExecutedBlock> blocks)
        {
            ExecutedBlock last = null;
            foreach (var block in blocks)
            {
                if (block == null)
                    continue;

                if (last == null || IsBefore(last, block))
                    last = block;
            }

            return last;
        }


        /// <summary>
        /// Get first of given blocks in current view ordering
        /// </summary>
        /// <param name="blocks">Blocks where first one is searched</param>
        /// <returns>First of given blocks</returns>
        internal ExecutedBlock EarliestBlock(IEnumerable<ExecutedBlock> blocks)
        {
            ExecutedBlock first = null;
            foreach (var block in blocks)
            {
                if (block == null)
                    continue;

                if (first == null || IsBefore(block,first))
                    first = block;
            }

            return first;
        }

        public void ShiftBehind(ExecutedBlock block, ExecutedBlock target)
        {
            //cut block from current position
            var nextBlock = NextBlock(block);
            var previousBlock = PreviousBlock(block);
            setEdge(previousBlock, nextBlock);

            //paste it behind target
            var nextTargetBlock = NextBlock(target);
            setEdge(target, block);
            setEdge(block, nextTargetBlock);

            var shiftTransform = block.Info.BlockTransformProvider.ShiftBehind(target.Info.BlockTransformProvider);
            Apply(shiftTransform);
        }

        public void AppendCall(ExecutedBlock block, CallEditInfo call)
        {
            var appendTransform = block.Info.BlockTransformProvider.AppendCall(call);
            Apply(appendTransform);
        }

        public void PrependCall(ExecutedBlock block, CallEditInfo call)
        {
            var prependTransform = block.Info.BlockTransformProvider.PrependCall(call);
            Apply(prependTransform);
        }

        public ExecutedBlock GetBlockInLevelOf(CallContext call, ExecutedBlock block)
        {
            var current = block;
            while (current != null)
            {
                if (call == current.Call)
                    return current;

                current = current.Call.CallingBlock;
            }

            //call is not in blocks call hierarchy
            return null;
        }

        public CallContext GetCommonCall(ExecutedBlock block1, ExecutedBlock block2)
        {
            //table of registered calls used for finding common call
            var calls = new HashSet<CallContext>();

            //register calls from blocks
            var current1 = block1.Call;
            var current2 = block2.Call;

            //common stepping because of optimiztion of same level cases
            while (current1 != null && current2 != null)
            {
                if (current1 != null)
                    if (calls.Add(current1))
                    {
                        return current1;
                    }
                    else
                    {
                        current1 = current1.Caller;
                    }

                if (current2 != null)
                    if (calls.Add(current2))
                    {
                        return current2;
                    }
                    else
                    {
                        current2 = current2.Caller;
                    }
            }

            //this could happened only if blocks are from different runs
            return null;
        }

        #endregion


        #region Private helpers

        private ExecutedBlock fromChanges(Dictionary<ExecutedBlock, ExecutedBlock> changes, ExecutedBlock changedBlock, ExecutedBlock defaultBlock)
        {
            ExecutedBlock block;
            if (!changes.TryGetValue(changedBlock, out block))
                block = defaultBlock;

            return block;
        }

        /// <summary>
        /// Set edge from block1 to block2 into view
        /// </summary>
        /// <param name="block1">Block which next will be block2</param>
        /// <param name="block2">Block which previous will be block1</param>
        private void setEdge(ExecutedBlock block1, ExecutedBlock block2)
        {
            if (block1 != null)
                _nextChanges[block1] = block2;

            if (block2 != null)
                _previouChanges[block2] = block1;
        }

        #endregion
    }
}
