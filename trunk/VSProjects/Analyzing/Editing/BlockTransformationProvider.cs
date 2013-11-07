using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class BlockTransformationProvider
    {
        public abstract Transformation ShiftBefore(BlockTransformationProvider provider);
        public abstract Transformation ShiftBehind(BlockTransformationProvider provider);
        public abstract Transformation PrependCall(CallEditInfo call);
        public abstract Transformation AppendCall(CallEditInfo call);
    }
}
