using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public class EmptyCallTransformProvider : CallTransformProvider
    {
        public override RemoveTransformProvider RemoveArgument(int argumentIndex)
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
    }
}
