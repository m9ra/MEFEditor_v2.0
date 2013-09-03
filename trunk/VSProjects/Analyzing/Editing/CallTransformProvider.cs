using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class CallTransformProvider:TransformProvider
    {
        public abstract RemoveTransformProvider RemoveArgument(int argumentIndex,bool keepSideEffect);
        public abstract Transformation RewriteArgument(int argumentIndex,ValueProvider valueProvider);
        public abstract Transformation AppendArgument(ValueProvider valueProvider);
        public abstract bool IsOptionalArgument(int argumentIndex);
        public abstract void SetOptionalArgument(int index);
    }
}
