using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Jump to specified target label instruction.
    /// </summary>
    class Jump : InstructionBase
    {
        /// <summary>
        /// The target label.
        /// </summary>
        internal readonly Label Target;

        /// <summary>
        /// Initializes a new instance of the <see cref="Jump" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        internal Jump(Label target)
        {
            Target = target;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            context.Jump(Target);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("jmp {0}", Target);
        }
    }
}
