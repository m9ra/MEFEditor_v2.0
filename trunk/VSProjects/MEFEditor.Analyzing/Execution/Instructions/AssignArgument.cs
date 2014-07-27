using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    class AssignArgument :  AssignBase
    {
        private readonly uint _argumentNumber;
        private readonly VariableName _targetVariable;

        internal AssignArgument(VariableName targetVariable, uint argumentNumber)
        {
            _argumentNumber = argumentNumber;
            _targetVariable = targetVariable;
        }

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

        public override string ToString()
        {
            return string.Format("mov_arg {0}, {1}", _targetVariable, _argumentNumber);
        }
    }
}
