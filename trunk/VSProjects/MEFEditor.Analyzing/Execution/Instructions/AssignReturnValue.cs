using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Last call return value to variable assign instruction.
    /// </summary>
    class AssignReturnValue : AssignBase
    {
        /// <summary>
        /// The target variable.
        /// </summary>
        private readonly VariableName _targetVariable;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignReturnValue" /> class.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        internal AssignReturnValue(VariableName targetVariable)
        {
            _targetVariable = targetVariable;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            context.SetValue(_targetVariable, context.LastReturnValue);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("mov_return {0}", _targetVariable);
        }
    }
}
