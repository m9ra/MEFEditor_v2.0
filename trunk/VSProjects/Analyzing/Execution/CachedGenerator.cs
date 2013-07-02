using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    class CachedGenerator:IInstructionGenerator
    {

        List<IInstruction> _instructions = new List<IInstruction>();
        
        public CachedGenerator(DirectMethod function)
        {         
            _instructions.Add(new DirectCall(function));
        }
        
        public void Generate(IEmitter emitter)
        {
            var e = emitter as CallEmitter;

            e.DirectEmit(_instructions);
        }

        public VersionedName Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
