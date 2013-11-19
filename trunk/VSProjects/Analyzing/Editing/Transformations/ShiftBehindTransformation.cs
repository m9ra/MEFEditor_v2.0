using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{
    class ShiftBehindTransformation:Transformation
    {
        private readonly ExecutedBlock _shifted;
        private readonly ExecutedBlock _target;

        internal ShiftBehindTransformation(ExecutedBlock shifted, ExecutedBlock target)
        {
            _shifted = shifted;
            _target = target;
        }

        protected override void apply(ExecutionView view)
        {
            //TODO correct block shifting
            //TODO change transformation semantic
            view.ShiftBehind(_shifted, _target);
        }

        protected override bool commit(ExecutionView view)
        {
            return true;
        }
    }
}
