using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Abstract class that describes providing of basic transformation on instruction blocks
    /// defined by <see cref="InstructionInfo" />.
    /// </summary>
    public abstract class TransformProvider
    {
        /// <summary>
        /// Provide remove provider of instruction block;
        /// </summary>
        /// <returns>Remove provider if available, <c>null</c> otherwise.</returns>
        public abstract RemoveTransformProvider Remove();
    }
}
