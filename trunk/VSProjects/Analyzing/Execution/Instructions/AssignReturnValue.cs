using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class AssignReturnValue:IInstruction
    {
        private readonly VariableName _targetVariable;
     

        internal AssignReturnValue(VariableName targetVariable)
        {            
            _targetVariable = targetVariable;
        }
        public void Execute(AnalyzingContext context)
        {
            context.SetValue(_targetVariable, context.LastReturnValue);
        }
    }
}
