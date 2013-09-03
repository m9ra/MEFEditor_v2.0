using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    class EmptyCallTransformProvider : CallTransformProvider
    {
        public override RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect)
        {
            return new EmptyRemoveProvider();
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valueProvider)
        {
            return new EmptyTransformation();
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {
            return new EmptyTransformation();
        }

        public override RemoveTransformProvider Remove()
        {
            return new EmptyRemoveProvider();
        }

        public override bool IsOptionalArgument(int argumentIndex)
        {
            return false;
        }

        public override void SetOptionalArgument(int index)
        {
            //nothing to do
        }
    }
}
