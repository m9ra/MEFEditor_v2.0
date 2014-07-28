using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Action that can be used for navigation at providing instruction
    /// </summary>
    public delegate void NavigationAction();

    /// <summary>
    /// Class CallTransformProvider.
    /// </summary>
    public abstract class CallTransformProvider : TransformProvider
    {
        /// <summary>
        /// Get navigation at start of given call if available.
        /// </summary>
        /// <returns><see cref="NavigationAction" /> if available, <c>null</c> otherwise.</returns>
        public abstract NavigationAction GetNavigation();

        /// <summary>
        /// Provide transformation that removes specified argument.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="keepSideEffect">if set to <c>true</c> keeps side effect of removed argument.</param>
        /// <returns>RemoveTransformProvider.</returns>
        public abstract RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect);

        /// <summary>
        /// Provide transformation that rewrites specified argument.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="valueProvider">The value provider.</param>
        /// <returns>Transformation.</returns>
        public abstract Transformation RewriteArgument(int argumentIndex, ValueProvider valueProvider);

        /// <summary>
        /// Provide transformation that appends specified argument.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <param name="valueProvider">The value provider.</param>
        /// <returns>Transformation.</returns>
        public abstract Transformation AppendArgument(int argumentIndex, ValueProvider valueProvider);

        /// <summary>
        /// Determines whether argument at given index is optional.
        /// </summary>
        /// <param name="argumentIndex">Index of the argument.</param>
        /// <returns><c>true</c> if argument is optional; otherwise, <c>false</c>.</returns>
        public abstract bool IsOptionalArgument(int argumentIndex);

        /// <summary>
        /// Sets optional flag for specified argument.
        /// </summary>
        /// <param name="index">The index of argument.</param>
        public abstract void SetOptionalArgument(int index);
    }
}
