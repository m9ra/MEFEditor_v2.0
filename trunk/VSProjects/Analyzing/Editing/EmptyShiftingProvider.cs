using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    class EmptyShiftingProvider:ShiftingTransformationProvider
    {
        public override Transformation ShiftBefore(ShiftingTransformationProvider provider)
        {
            return new EmptyTransformation();
        }

        public override Transformation ShiftBehind(ShiftingTransformationProvider provider)
        {
            return new EmptyTransformation();
        }
    }
}
