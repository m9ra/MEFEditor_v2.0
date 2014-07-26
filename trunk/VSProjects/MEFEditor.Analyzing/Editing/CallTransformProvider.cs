using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    /// <summary>
    /// Action that can be used for navigation at providing instruction
    /// </summary>
    public delegate void NavigationAction();

    public abstract class CallTransformProvider : TransformProvider
    {
        /// <summary>
        /// Get navigation at start of given call if available
        /// </summary>
        /// <returns><see cref="NavigationAction"/> if available, <c>null</c> otherwise</returns>
        public abstract NavigationAction GetNavigation();
        public abstract RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect);
        public abstract Transformation RewriteArgument(int argumentIndex, ValueProvider valueProvider);
        public abstract Transformation AppendArgument(int argumentIndex, ValueProvider valueProvider);
        public abstract bool IsOptionalArgument(int argumentIndex);
        public abstract void SetOptionalArgument(int index);
    }
}
