using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public delegate void EmitDirector(EmitterBase emitter);


    class DirectedGenerator : GeneratorBase
    {
        private readonly EmitDirector _director;

        public DirectedGenerator(EmitDirector director)
        {
            _director = director;
        }

        protected override void generate(EmitterBase emitter)
        {
            _director(emitter);
        }
    }
}
