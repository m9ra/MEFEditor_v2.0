using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem
{
    public class MachineSettings:IMachineSettings
    {
        public InstanceInfo GetLiteralInfo(Type literalType)
        {
            return new InstanceInfo(literalType.FullName);
        }

        public InstanceInfo GetSharedInstanceInfo(string typeFullname)
        {
            return new InstanceInfo(typeFullname);
        }

        public bool IsTrue(Instance condition)
        {
            return (bool)condition.DirectValue;
        }
    }
}
