using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class LateReturnInitialization:IInstruction
    {
        private readonly VariableName _targetVariable;
                    
        
        internal LateReturnInitialization(VariableName targetVariable)
        {
            _targetVariable = targetVariable;            
        }

        public void Execute(AnalyzingContext context)
        {
            if (context.Contains(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //initialize shared instance
            context.SetValue(_targetVariable, context.LastReturnValue);
        }
    }
}
