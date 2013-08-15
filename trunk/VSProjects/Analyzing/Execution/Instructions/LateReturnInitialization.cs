using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class LateReturnInitialization<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {
        private readonly VariableName _targetVariable;
                    
        
        internal LateReturnInitialization(VariableName targetVariable)
        {
            _targetVariable = targetVariable;            
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            if (context.Contains(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //initialize shared instance
            context.SetValue(_targetVariable, context.LastReturnValue);
        }

        public override string ToString()
        {
            return string.Format("late_return {0}", _targetVariable);
        }
    }
}
