using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace UnitTesting.Analyzing_TestUtils
{

    delegate void EmitDirector(IEmitter emitter);

    static class ExecutionUtils
    {
        public static void Run(EmitDirector director)
        {
            var machine = new Machine();
            var loader=new TestLoader(director);
            machine.Run(loader);
        }
    }
}
