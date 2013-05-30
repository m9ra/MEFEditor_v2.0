using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Call:IInstruction
    {
        private readonly VersionedName _methodGeneratorName;
        private readonly VariableName[] _arguments;

        internal Call(VersionedName methodGeneratorName,IEnumerable<VariableName> arguments)
        {
            _methodGeneratorName = methodGeneratorName;
            _arguments = arguments.ToArray();
        }

        public void Execute(Context context)
        {
            var generator = context.GetGenerator(_methodGeneratorName);
            context.FetchInstructions(generator);
            context.PrepareArguments(_arguments);            
        }
    }
}
