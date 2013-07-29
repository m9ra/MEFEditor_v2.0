using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.CSharp.Compiling
{
    class Context
    {
        public readonly IEmitter<MethodID, InstanceInfo> Emitter;

        public Context(IEmitter<MethodID, InstanceInfo> emitter)
        {
            Emitter = emitter;
        }

    }
}
