using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    class AssignNewObject : AssignBase
    {
        private readonly VariableName _targetVariable;
        private readonly InstanceInfo _objectInfo;

        internal AssignNewObject(VariableName targetVariable, InstanceInfo objectInfo)
        {
            _objectInfo = objectInfo;
            _targetVariable = targetVariable;
        }

        public override void Execute(AnalyzingContext context)
        {
            var newInstance = context.Machine.CreateInstance(_objectInfo);
            if (RemoveProvider != null)
                newInstance.HintCreationNavigation(RemoveProvider.GetNavigation());

            newInstance.CreationBlock = context.CurrentCall.CurrentBlock;

            context.SetValue(_targetVariable, newInstance);
        }

        public override string ToString()
        {
            return string.Format("mov_new {0}, {1}", _targetVariable, _objectInfo);
        }
    }
}
