using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing.Execution.Instructions
{
    class Call : InstructionBase
    {
        private readonly MethodID _method;

        private readonly Arguments _arguments;

        internal CallTransformProvider TransformProvider { get; set; }

        internal Call(MethodID methodGeneratorName, Arguments arguments)
        {
            _method = methodGeneratorName;
            _arguments = arguments;
        }

        public override void Execute(AnalyzingContext context)
        {
            var argumentValues = context.GetArguments(_arguments);
           
            context.FetchCall(_method,argumentValues);
        }

        public override string ToString()
        {
            return string.Format("prepare_call {0}\ncall {1}", _arguments, _method);
        }
    }
}
