using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Delegate used for lazy creating <see cref="ExecutionViewData"/> stored within <see cref="ExecutionView"/>
    /// </summary>
    /// <typeparam name="T">Type of stored data</typeparam>
    /// <returns>Stored data.</returns>
    public delegate T ViewDataProvider<T>()
        where T : ExecutionViewData;

    /// <summary>
    /// Represent a view on chain of <see cref="ExecutedBlock"/> objects. Also provide
    /// transformation services on the chain, that can be applied into source instructions.
    /// </summary>
    public class ExecutionView
    {
        /// <summary>
        /// Gets the abort message describing reason of view aborting.
        /// </summary>
        /// <value>The abort message.</value>
        public string AbortMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is aborted.
        /// </summary>
        /// <value><c>true</c> if this instance is aborted; otherwise, <c>false</c>.</value>
        public bool IsAborted { get { return AbortMessage != null; } }

        /// <summary>
        /// Gets a value indicating whether this instance is committed.
        /// </summary>
        /// <value><c>true</c> if this instance is committed; otherwise, <c>false</c>.</value>
        public bool IsCommitted { get; private set; }

        /// <summary>
        /// Provider of remove transformation on <see cref="Instances"/> from current view.
        /// </summary>
        private readonly InstanceRemoveProvider _instanceRemoveProvider;

        /// <summary>
        /// The result of analysis represented by current view.
        /// </summary>
        private readonly AnalyzingResult _result;

        /// <summary>
        /// The data stored by current view.
        /// </summary>
        private readonly ExecutionViewDataHandler _viewData = new ExecutionViewDataHandler();

        /// <summary>
        /// Index of next <see cref="ExecutedBlock"/> transformations.
        /// </summary>
        private readonly Dictionary<ExecutedBlock, ExecutedBlock> _nextChanges = new Dictionary<ExecutedBlock, ExecutedBlock>();

        /// <summary>
        /// Index of previous <see cref="ExecutedBlock"/> transformations.
        /// </summary>
        private readonly Dictionary<ExecutedBlock, ExecutedBlock> _previouChanges = new Dictionary<ExecutedBlock, ExecutedBlock>();

        /// <summary>
        /// The list of all applied transformations.
        /// </summary>
        private List<Transformation> _appliedTransformations = new List<Transformation>();

        /// <summary>
        /// Gets the entry block of analysis.
        /// </summary>
        /// <value>The entry block.</value>
        public ExecutedBlock EntryBlock { get { return _result.EntryContext.EntryBlock; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionView"/> class.
        /// </summary>
        /// <param name="result">The result of analysis represented by current view.</param>
        internal ExecutionView(AnalyzingResult result)
        {
            _result = result;
            _instanceRemoveProvider = new InstanceRemoveProvider(result.EntryContext);
        }

        /// <summary>
        /// Abort current view with given message.
        /// </summary>
        /// <param name="abortMessage">Message with description why view is aborted.</param>
        /// <returns><c>null</c> - because of shorter notation possibility.</returns>
        /// <exception cref="System.NotSupportedException">Cannot abort twice</exception>
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
        /// Apply transformation on current view.
        /// </summary>
        /// <param name="transformation">Transformation to be applied.</param>
        public void Apply(Transformation transformation)
        {
            if (!IsAborted)
            {
                _appliedTransformations.Add(transformation);
                transformation.Apply(this);
            }
        }

        /// <summary>
        /// Commit all contained view data.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// Cannot commit when aborted
        /// or
        /// Cannot commit view twice
        /// </exception>
        public void Commit()
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot commit when aborted");
            }

            if (IsCommitted)
            {
                throw new NotSupportedException("Cannot commit view twice");
            }

            _viewData.Commit();

            _appliedTransformations = null;

            _result.ReportViewCommit(this);
        }


        /// <summary>
        /// Clone current view into view that is independent on current view.
        /// <remarks>Only one view in views cloning hierarchy can be committed</remarks>.
        /// </summary>
        /// <returns>Cloned view.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Cannot clone aborted view
        /// or
        /// Cannot clone committed view
        /// </exception>
        public ExecutionView Clone()
        {
            if (IsAborted)
                throw new NotSupportedException("Cannot clone aborted view");

            if (IsCommitted)
                throw new NotSupportedException("Cannot clone committed view");

            var clone = new ExecutionView(_result);
            clone._appliedTransformations.AddRange(_appliedTransformations);
            clone._viewData.FillFrom(_viewData);

            return clone;
        }


        /// <summary>
        /// Get data stored in current view for given key and type.
        /// If no matching data are found, new data is created via provider.
        /// </summary>
        /// <typeparam name="T">Type of searched data.</typeparam>
        /// <param name="key">Key of data - because multiple sources can store data with same type.</param>
        /// <param name="provider">Provider used for data creation.</param>
        /// <returns>Stored data, created data, or null if the data is not found and provider is not present.</returns>
        public T Data<T>(object key, ViewDataProvider<T> provider = null)
            where T : ExecutionViewData
        {
            return _viewData.Data<T>(key, provider);
        }

        #region View Transformation API

        /// <summary>
        /// Removes the specified instance.
        /// </summary>
        /// <param name="instance">The instance to be removed.</param>
        /// <returns><c>true</c> if instance has been removed, <c>false</c> otherwise.</returns>
        public bool Remove(Instance instance)
        {
            return _instanceRemoveProvider.Remove(instance, this);
        }

        /// <summary>
        /// Determines whether given instance can be removed.
        /// </summary>
        /// <param name="instance">The instance to be removed.</param>
        /// <returns><c>true</c> if given instance can be removed; otherwise, <c>false</c>.</returns>
        public bool CanRemove(Instance instance)
        {
            return _instanceRemoveProvider.CanRemove(instance, this);
        }

        /// <summary>
        /// Get <see cref="ExecutedBlock"/> after given block in current view.
        /// </summary>
        /// <param name="block">The block which next block is required.</param>
        /// <returns>Next block.</returns>
        public ExecutedBlock NextBlock(ExecutedBlock block)
        {
            return fromChanges(_nextChanges, block, block.NextBlock);
        }

        /// <summary>
        /// Get <see cref="ExecutedBlock"/> before given block in current view.
        /// </summary>
        /// <param name="block">The block which previous block is required.</param>
        /// <returns>Previous block.</returns>
        public ExecutedBlock PreviousBlock(ExecutedBlock block)
        {
            return fromChanges(_previouChanges, block, block.PreviousBlock);
        }

        /// <summary>
        /// Get enumeration of <see cref="Instance"/> affected by given block in current view.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>Affected instances</returns>
        public IEnumerable<Instance> AffectedInstances(ExecutedBlock block)
        {
            return block.AffectedInstances;
        }

        /// <summary>
        /// Get scope starts of instance for given block in current view.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="instance">Instance which scope starts are requested.</param>
        /// <returns>Enumeration of variables which scope starts at given block.</returns>
        internal IEnumerable<VariableName> ScopeStarts(ExecutedBlock block, Instance instance)
        {
            return block.ScopeStarts(instance);
        }

        /// <summary>
        /// Get scope ends of instance for given block in current view.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="instance">Instance which scope ends are requested.</param>
        /// <returns>Enumeration of variables which scope ends at given block.</returns>
        internal IEnumerable<VariableName> ScopeEnds(ExecutedBlock block, Instance instance)
        {
            return block.ScopeEnds(instance);
        }

        /// <summary>
        /// Get last <see cref="ExecutedBlock"/> where given instance was called.
        /// </summary>
        /// <param name="instance">The instance which last call block is requested.</param>
        /// <returns>Last call block.</returns>
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
        /// Determine if block1 is in current view before block2.
        /// </summary>
        /// <param name="block1">The block1.</param>
        /// <param name="block2">The block2.</param>
        /// <returns><c>true</c> if the specified block1 is before; otherwise, <c>false</c>.</returns>
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
        /// Get last of given blocks in current view ordering.
        /// </summary>
        /// <param name="blocks">Blocks where last one is searched.</param>
        /// <returns>Last of given blocks.</returns>
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
        /// Get first of given blocks in current view ordering.
        /// </summary>
        /// <param name="blocks">Blocks where first one is searched.</param>
        /// <returns>First of given blocks.</returns>
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

        /// <summary>
        /// Shift given block behind target block
        /// </summary>
        /// <param name="block">The shifted block.</param>
        /// <param name="target">The target block.</param>
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

        /// <summary>
        /// Append call after given block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="call">The appended call.</param>
        public void AppendCall(ExecutedBlock block, CallEditInfo call)
        {
            var appendTransform = block.Info.BlockTransformProvider.AppendCall(call);
            Apply(appendTransform);
        }

        /// <summary>
        /// Prepends the call before given block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="call">The prepended call.</param>
        public void PrependCall(ExecutedBlock block, CallEditInfo call)
        {
            var prependTransform = block.Info.BlockTransformProvider.PrependCall(call);
            Apply(prependTransform);
        }

        /// <summary>
        /// Gets ancestor block (of given block) that is contained in given call.
        /// </summary>
        /// <param name="call">The call.</param>
        /// <param name="block">The block.</param>
        /// <returns>Ancestor block.</returns>
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

        /// <summary>
        /// Gets call where appeared ancestors of both given blocks.
        /// </summary>
        /// <param name="block1">The block1.</param>
        /// <param name="block2">The block2.</param>
        /// <returns>Desired call.</returns>
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

        /// <summary>
        /// Get block according to given changes log.
        /// </summary>
        /// <param name="changes">The changes log.</param>
        /// <param name="changedBlock">The changed block.</param>
        /// <param name="defaultBlock">The default block.</param>
        /// <returns>Desired block.</returns>
        private ExecutedBlock fromChanges(Dictionary<ExecutedBlock, ExecutedBlock> changes, ExecutedBlock changedBlock, ExecutedBlock defaultBlock)
        {
            ExecutedBlock block;
            if (!changes.TryGetValue(changedBlock, out block))
                block = defaultBlock;

            return block;
        }

        /// <summary>
        /// Set edge from block1 to block2 into view.
        /// </summary>
        /// <param name="block1">Block which next will be block2.</param>
        /// <param name="block2">Block which previous will be block1.</param>
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
