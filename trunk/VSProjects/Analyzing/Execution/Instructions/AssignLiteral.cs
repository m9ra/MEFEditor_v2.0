using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class AssignLiteral<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {        
        private readonly VariableName _targetVariable;
        private readonly Instance _literal;

        internal AssignLiteral(VariableName targetVariable, Instance literal)
        {
            _literal = literal;
            _targetVariable = targetVariable;
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {            
            context.SetValue(_targetVariable, _literal);
        }
    }
}
