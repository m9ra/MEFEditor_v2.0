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
        private readonly DirectMethod _method;

        public DirectGenerator(DirectMethod directMethod)
        {
            _method = directMethod;
        }

        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(_method);
        }
    }
}
