using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Common base class for all assign instructions.
    /// It provides common access to <see cref="RemoveProvider"/>.
    /// </summary>
    abstract class AssignBase : InstructionBase
    {
        /// <summary>
        /// Remove provider that can be set for assigns.
        /// </summary>
        internal RemoveTransformProvider RemoveProvider { get; set; }
    }
}
