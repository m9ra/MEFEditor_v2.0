using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{
    class ShiftBehindTransformation : Transformation
    {
        /// <summary>
        /// Block that will be correctly shifted so it is behind given target
        /// </summary>
        private readonly ExecutedBlock _shifted;

        /// <summary>
        /// Target that needs shifted block to be behind the target
        /// </summary>
        private readonly ExecutedBlock _target;

        internal ShiftBehindTransformation(ExecutedBlock shifted, ExecutedBlock target)
        {
            _shifted = shifted;
            _target = target;
        }

        protected override void apply()
        {
            //TODO correct block shifting
            View.ShiftBehind(_shifted, _target);
        }
    }
}
