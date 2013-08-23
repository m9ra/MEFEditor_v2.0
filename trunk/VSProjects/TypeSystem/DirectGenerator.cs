using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class DirectGenerator:GeneratorBase
    {
        private readonly DirectMethod<MethodID, InstanceInfo> _method;

        public DirectGenerator(DirectMethod<MethodID, InstanceInfo> directMethod)
        {
            _method = directMethod;
        }

        protected override void generate(EmitterBase<MethodID, InstanceInfo> emitter)
        {
            emitter.DirectInvoke(_method);
        }
    }
}
