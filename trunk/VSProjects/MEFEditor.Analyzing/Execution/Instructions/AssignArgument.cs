using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Assign from argument to variable instruction.
    /// </summary>
    class AssignArgument :  AssignBase
    {
        /// <summary>
        /// The argument number.
        /// </summary>
        private readonly uint _argumentNumber;

        /// <summary>
        /// The target variable.
        /// </summary>
        private readonly VariableName _targetVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignArgument" /> class.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="argumentNumber">The argument number.</param>
        internal AssignArgument(VariableName targetVariable, uint argumentNumber)
        {
            _argumentNumber = argumentNumber;
            _targetVariable = targetVariable;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        /// <exception cref="System.NotSupportedException">Argument is not available</exception>
        public override void Execute(AnalyzingContext context)
        {
            if (context.CurrentArguments.Length > 0)
            {
                var sourceValue = context.CurrentArguments[_argumentNumber];
                context.SetValue(_targetVariable, sourceValue);
            }
            else
            {
                throw new NotSupportedException("Argument is not available");
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("mov_arg {0}, {1}", _targetVariable, _argumentNumber);
        }
    }
}
