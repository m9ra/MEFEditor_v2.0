using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
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
            return block.NextBlock;
        }

        public ExecutedBlock PreviousBlock(ExecutedBlock block)
        {
            return block.PreviousBlock;
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

        public void ShiftBehind(ExecutedBlock block, ExecutedBlock target)
        {
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

        #endregion
    }
}
