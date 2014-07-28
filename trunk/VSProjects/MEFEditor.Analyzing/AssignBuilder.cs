using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Builder of instruction info of assign instructions.
    /// </summary>
    public class AssignBuilder
    {
        /// <summary>
        /// The _instruction
        /// </summary>
        readonly AssignBase _instruction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignBuilder"/> class.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        internal AssignBuilder(AssignBase instruction)
        {
            _instruction = instruction;
        }

        /// <summary>
        /// Sets the remove provider.
        /// </summary>
        /// <value>The remove provider.</value>
        public RemoveTransformProvider RemoveProvider
        {
            set
            {
                _instruction.RemoveProvider = value;
            }
        }
    }
}
