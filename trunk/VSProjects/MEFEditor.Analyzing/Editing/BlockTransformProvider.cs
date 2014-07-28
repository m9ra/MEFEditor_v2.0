using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Abstract class that describes providing transformation on instruction blocks
    /// defined by <see cref="InstructionInfo"/>.
    /// </summary>
    public abstract class BlockTransformProvider
    {
        /// <summary>
        /// Shifts current instruction block before block described by given provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>Shifting transformation.</returns>
        public abstract Transformation ShiftBefore(BlockTransformProvider provider);

        /// <summary>
        /// Shifts current instruction block behind block described by given provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>Shifting transformation.</returns>
        public abstract Transformation ShiftBehind(BlockTransformProvider provider);

        /// <summary>
        /// Add described call before current instruction block.
        /// </summary>
        /// <param name="call">Definition of created call.</param>
        /// <returns>Call creation transformation.</returns>
        public abstract Transformation PrependCall(CallEditInfo call);

        /// <summary>
        /// Add described call after current instruction block.
        /// </summary>
        /// <param name="call">Definition of created call.</param>
        /// <returns>Call creation transformation.</returns>
        public abstract Transformation AppendCall(CallEditInfo call);

        /// <summary>
        /// Gets the navigation to current instruction block.
        /// </summary>
        /// <returns>Navigation action if available, <c>null</c> otherwise.</returns>
        public abstract NavigationAction GetNavigation();
    }
}
