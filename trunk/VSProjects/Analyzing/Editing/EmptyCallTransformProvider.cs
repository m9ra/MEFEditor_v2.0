using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public class EmptyCallTransformProvider : CallTransformProvider
    {
        public override Transformation RemoveArgument(int argumentIndex)
        {
            return new EmptyTransformation();
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valueProvider)
        {
            return new EmptyTransformation();
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {
            return new EmptyTransformation();
        }
    }
}
