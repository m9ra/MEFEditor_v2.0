using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution.Instructions;

namespace Analyzing
{
    public class AssignBuilder<MethodID, InstanceInfo>
    {
        readonly AssignBase<MethodID, InstanceInfo> _instruction;
        internal AssignBuilder(AssignBase<MethodID,InstanceInfo> instruction)
        {
            _instruction = instruction;
        }

        public RemoveTransformProvider RemoveProvider
        {
            set
            {
                _instruction.RemoveProvider = value;
            }
        }
    }
}
