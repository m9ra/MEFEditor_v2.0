using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Instruction for invoking native .NET methods in context of analysis.
    /// </summary>
    class DirectInvoke : InstructionBase
    {
        /// <summary>
        /// The native method.
        /// </summary>
        DirectMethod _method;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInvoke" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        public DirectInvoke(DirectMethod method)
        {
            _method = method;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            _method(context);            
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("direct_invoke {0}", _method);
        }
    }
}
