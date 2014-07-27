using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.UnitTesting.Analyzing_TestUtils
{
    class EmitDirectorGenerator:GeneratorBase
    {
        private readonly EmitDirector _director;

        internal EmitDirectorGenerator(EmitDirector director)
        {
            _director = director;
        }

        protected override void generate(EmitterBase emitter)
        {
            _director(emitter);
        }
    }
}
