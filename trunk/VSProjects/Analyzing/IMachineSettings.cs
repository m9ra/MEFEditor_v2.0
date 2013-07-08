using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public delegate void DirectMethod<MethodID, InstanceInfo>(AnalyzingContext<MethodID, InstanceInfo> context);


    public interface IMachineSettings<InstanceInfo>
    {
        InstanceInfo GetLiteralInfo(Type literalType);

        InstanceInfo GetSharedInstanceInfo(string typeFullname);

        bool IsTrue(Instance condition);
    }
}
