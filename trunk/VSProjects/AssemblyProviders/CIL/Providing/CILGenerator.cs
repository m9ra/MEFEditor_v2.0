using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.CIL.Providing
{
    class CILGenerator : GeneratorBase
    {
        private readonly MethodDefinition _method;

        private readonly TypeMethodInfo _info;

        private readonly TypeServices _services;

        internal CILGenerator(MethodDefinition method, TypeMethodInfo methodInfo, TypeServices services)
        {
            if (services == null)
                throw new ArgumentNullException("services");

            _method = method;
            _info = methodInfo;
            _services = services;
        }

        protected override void generate(EmitterBase emitter)
        {
            var method = new CILMethod(_method);
            Compiler.GenerateInstructions(method, _info, emitter, _services);
        }
    }
}
