using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    class DirectGenerator:IInstructionGenerator
    {
        private readonly DirectMethod<MethodID, InstanceInfo> _method;

        internal DirectGenerator(DirectMethod<MethodID, InstanceInfo> directMethod)
        {
            _method = directMethod;
        }

        public void Generate(IEmitter<MethodID, InstanceInfo> emitter)
        {
            emitter.DirectInvoke(_method);
        }
    }
}
