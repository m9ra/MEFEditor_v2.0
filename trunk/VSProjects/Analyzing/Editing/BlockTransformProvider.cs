using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class BlockTransformProvider
    {
        public abstract Transformation ShiftBefore(BlockTransformProvider provider);
        public abstract Transformation ShiftBehind(BlockTransformProvider provider);
        public abstract Transformation PrependCall(CallEditInfo call);
        public abstract Transformation AppendCall(CallEditInfo call);
    }
}
