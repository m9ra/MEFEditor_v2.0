using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Return:IInstruction
    {
        readonly VariableName _sourceVariable;
     

        internal Return(VariableName sourceVariable)
        {            
            _sourceVariable = sourceVariable;
        }

        public void Execute(AnalyzingContext context)
        {
            var returnValue=context.GetValue(_sourceVariable);
            context.Return(returnValue);
        }
    }
}
