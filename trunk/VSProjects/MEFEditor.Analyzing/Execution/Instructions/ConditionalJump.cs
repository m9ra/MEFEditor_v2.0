using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Jump to label when condition is satisfied instruction.
    /// </summary>
    class ConditionalJump : InstructionBase
    {
        /// <summary>
        /// The jump target.
        /// </summary>
        internal readonly Label Target;

        /// <summary>
        /// The jump condition.
        /// </summary>
        private readonly VariableName _condition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalJump" /> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="target">The target.</param>
        internal ConditionalJump(VariableName condition,Label target)
        {
            _condition = condition;
            Target = target;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            if (context.IsTrue(_condition))
            {
                context.Jump(Target);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("jmp {0} if {1}", Target, _condition);
        }
    }
}
