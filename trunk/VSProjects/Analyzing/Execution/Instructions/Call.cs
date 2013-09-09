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

        internal CallTransformProvider TransformProvider { get; set; }
        
        internal Call(MethodID methodGeneratorName)
        {
            _method = methodGeneratorName;            
        }

        public override void Execute(AnalyzingContext context)
        {
            var generator = context.GetGenerator(_method);
            context.FetchCallInstructions(_method,generator);            
        }

        public override string ToString()
        {
            return string.Format("call {0}", _method);
        }
    }
}
