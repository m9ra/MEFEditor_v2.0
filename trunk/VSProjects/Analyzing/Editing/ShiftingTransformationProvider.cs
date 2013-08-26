using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class ShiftingTransformationProvider
    {
        public abstract Transformation ShiftBefore(ShiftingTransformationProvider provider);
        public abstract Transformation ShiftBehind(ShiftingTransformationProvider provider);
    }
}
