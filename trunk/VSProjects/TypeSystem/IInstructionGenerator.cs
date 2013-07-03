using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public interface IInstructionGenerator:Analyzing.IInstructionGenerator<MethodID,InstanceInfo>
    {
    }
}
