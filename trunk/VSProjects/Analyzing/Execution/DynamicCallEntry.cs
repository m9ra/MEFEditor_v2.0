using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class DynamicCallEntry
    {
        internal readonly MethodID Method;

        internal readonly GeneratorBase Generator;

        internal readonly Instance[] Arguments;

        internal DynamicCallEntry NextDynamicCall;

        internal DynamicCallEntry(MethodID method, GeneratorBase generator, Instance[] arguments)
        {
            Method = method;
            Generator = generator;
            Arguments = arguments;
        }

        internal DynamicCallEntry LastCall
        {
            get
            {
                if (NextDynamicCall == null)
                    return this;

                return NextDynamicCall.LastCall;
            }
        }
    }
}
