using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
{
    public delegate T ViewDataProvider<T>()
        where T : ICloneable;

    /// <summary>
    /// Is available when transformation is applied to provide transformation services
    /// </summary>
    public class ExecutionView
    {
        public string AbortMessage { get; private set; }

        public bool IsAborted { get { return AbortMessage != null; } }

        private readonly RemoveHandler _removeHandler;

        private readonly AnalyzingResult _result;

        private readonly Dictionary<Type, ICloneable> _viewData = new Dictionary<Type, ICloneable>();

        private List<Transformation> _appliedTransformations = new List<Transformation>();

        public ExecutedBlock EntryBlock { get { return _result.EntryContext.EntryBlock; } }

        internal ExecutionView(AnalyzingResult result, RemoveHandler removeHandler)
        {
            _result = result;
            _removeHandler = removeHandler;
        }

        public object Abort(string abortMessage)
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot abort twice");
            }
            AbortMessage = abortMessage;

            return null;
        }

        public void Apply(Transformation transformation)
        {
            if (!IsAborted)
            {
                _appliedTransformations.Add(transformation);
                transformation.Apply(this);
            }
        }

        public void Commit()
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot commit when aborted");
            }

            foreach (var transform in _appliedTransformations)
            {
                if (!transform.Commit(this))
                {
                    throw new NotSupportedException("Commiting transformation failed, sources can be in inconsistent state");
                }
            }

            _appliedTransformations = null;

            _result.ReportViewCommit(this);
        }

        public bool Remove(Instance instance)
        {
            return _removeHandler(instance, this);
        }

        public ExecutionView Clone()
        {
            if (IsAborted)
                throw new NotSupportedException("Cannot clone aborted view");

            var clone = new ExecutionView(_result, _removeHandler);
            clone._appliedTransformations.AddRange(_appliedTransformations);

            foreach (var data in _viewData)
            {
                clone._viewData[data.Key] = data.Value.Clone() as ICloneable;
            }

            return clone;
        }

        #region View Transformation API

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

        #endregion

        public T Data<T>(ViewDataProvider<T> provider = null)
            where T : ICloneable
        {
            var type = typeof(T);

            ICloneable data;
            if (!_viewData.TryGetValue(type, out data))
            {
                if (provider == null)
                {
                    return default(T);
                }
                else
                {
                    _viewData[type] = data = provider();
                }
            }
            return (T)data;
        }


    }
}
