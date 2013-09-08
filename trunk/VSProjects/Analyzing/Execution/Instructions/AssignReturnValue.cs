using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class AssignReturnValue : AssignBase
    {
        private readonly VariableName _targetVariable;
     

        internal AssignReturnValue(VariableName targetVariable)
        {            
            _targetVariable = targetVariable;
        }
        public override void Execute(AnalyzingContext context)
        {
            context.SetValue(_targetVariable, context.LastReturnValue);
        }

        public override string ToString()
        {
            return string.Format("mov_return {0}",  _targetVariable);
        }
    }
}
