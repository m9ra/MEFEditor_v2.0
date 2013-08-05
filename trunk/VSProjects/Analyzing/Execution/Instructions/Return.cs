using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Return<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {
        readonly VariableName _sourceVariable;
     

        internal Return(VariableName sourceVariable)
        {            
            _sourceVariable = sourceVariable;
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            var returnValue=context.GetValue(_sourceVariable);
            context.Return(returnValue);
        }

        public override string ToString()
        {
            return string.Format("ret {0}", _sourceVariable);
        }
    }
}
