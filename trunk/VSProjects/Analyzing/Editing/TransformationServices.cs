using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
{
    /// <summary>
    /// Is available when transformation is applied to provide transformation services
    /// </summary>
    public class TransformationServices
    {
        public string AbortMessage { get; private set; }

        public bool IsAborted { get { return AbortMessage != null; } }

        private readonly RemoveHandler _removeHandler;

        private readonly AnalyzingResult _result;

        private List<Transformation> _appliedTransformations = new List<Transformation>();

        internal TransformationServices(AnalyzingResult result, RemoveHandler removeHandler)
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
                if (!transform.Commit())
                {
                    throw new NotSupportedException("Commiting transformation failed, sources can be in inconsistent state");
                }
            }

            _appliedTransformations = null;

            _result.ReportTransformationCommit();
        }

        public bool Remove(Instance instance)
        {
            return _removeHandler(instance, this);
        }
    }
}
