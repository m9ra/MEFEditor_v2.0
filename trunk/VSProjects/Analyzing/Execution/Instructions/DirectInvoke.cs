﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class DirectInvoke<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {
        DirectMethod<MethodID, InstanceInfo> _call;
        public DirectInvoke(DirectMethod<MethodID, InstanceInfo> call)
        {
            _call = call;
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            _call(context);            
        }
    }
}
