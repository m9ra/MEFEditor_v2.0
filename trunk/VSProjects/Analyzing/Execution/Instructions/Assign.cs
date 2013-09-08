using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Assign : AssignBase
    {
        private readonly VariableName _sourceVariable;
        private readonly VariableName _targetVariable;

        internal Assign(VariableName targetVariable, VariableName sourceVariable)
        {
            _sourceVariable = sourceVariable;
            _targetVariable = targetVariable;
        }

        public override void Execute(AnalyzingContext context)
        {
            var sourceValue = context.GetValue(_sourceVariable);
            context.SetValue(_targetVariable, sourceValue);
        }

        public override string ToString()
        {
            return string.Format("mov {0}, {1}", _targetVariable, _sourceVariable);
        }
    }
}
