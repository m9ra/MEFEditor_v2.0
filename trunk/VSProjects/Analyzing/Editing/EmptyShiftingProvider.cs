using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing.Transformations;

namespace Analyzing.Editing
{
    class EmptyShiftingProvider : BlockTransformationProvider
    {
        public override Transformation ShiftBefore(BlockTransformationProvider provider)
        {
            return new EmptyTransformation();
        }

        public override Transformation ShiftBehind(BlockTransformationProvider provider)
        {
            return new EmptyTransformation();
        }

        public override Transformation PrependCall(CallEditInfo call)
        {
            return new EmptyTransformation();
        }

        public override Transformation AppendCall(CallEditInfo call)
        {
            return new EmptyTransformation();
        }
    }
}
