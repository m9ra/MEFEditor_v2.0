using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing.Transformations;

namespace MEFEditor.Analyzing.Editing
{
    class EmptyShiftingProvider : BlockTransformProvider
    {
        public override Transformation ShiftBefore(BlockTransformProvider provider)
        {
            return new EmptyTransformation();
        }

        public override Transformation ShiftBehind(BlockTransformProvider provider)
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

        public override NavigationAction GetNavigation()
        {
            return null;
        }
    }
}
