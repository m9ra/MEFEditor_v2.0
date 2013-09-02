using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class AssignNewObject<MethodID, InstanceInfo> : AssignBase<MethodID, InstanceInfo>
    {        
        private readonly VariableName _targetVariable;
        private readonly InstanceInfo _objectInfo;

        internal AssignNewObject(VariableName targetVariable, InstanceInfo objectInfo)
        {
            _objectInfo = objectInfo;
            _targetVariable = targetVariable;
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {            
            context.SetValue(_targetVariable, context.CreateInstance(_objectInfo));
        }

        public override string ToString()
        {
            return string.Format("mov_new {0}, {1}", _targetVariable, _objectInfo);
        }
    }
}
