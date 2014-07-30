using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Instruction for returning call value from variable.
    /// </summary>
    class Return : InstructionBase
    {
        /// <summary>
        /// The source variable.
        /// </summary>
        readonly VariableName _sourceVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Return" /> class.
        /// </summary>
        /// <param name="sourceVariable">The source variable.</param>
        internal Return(VariableName sourceVariable)
        {
            _sourceVariable = sourceVariable;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            var returnValue = context.GetValue(_sourceVariable);
            context.Return(returnValue);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("ret {0}", _sourceVariable);
        }
    }
}
