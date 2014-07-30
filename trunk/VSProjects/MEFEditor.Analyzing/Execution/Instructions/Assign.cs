using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Assign between two variables instruction.
    /// </summary>
    class Assign : AssignBase
    {
        /// <summary>
        /// The source variable.
        /// </summary>
        private readonly VariableName _sourceVariable;

        /// <summary>
        /// The target variable.
        /// </summary>
        private readonly VariableName _targetVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Assign" /> class.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="sourceVariable">The source variable.</param>
        internal Assign(VariableName targetVariable, VariableName sourceVariable)
        {
            _sourceVariable = sourceVariable;
            _targetVariable = targetVariable;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            var sourceValue = context.GetValue(_sourceVariable);

            context.SetValue(_targetVariable, sourceValue);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("mov {0}, {1}", _targetVariable, _sourceVariable);
        }
    }
}
