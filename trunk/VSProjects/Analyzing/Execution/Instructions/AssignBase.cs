using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing.Execution.Instructions
{
    abstract class AssignBase<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {
        internal RemoveTransformProvider RemoveProvider { get; set; }
    }
}
