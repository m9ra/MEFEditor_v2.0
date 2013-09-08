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
        private readonly VersionedName _methodGeneratorName;

        internal CallTransformProvider TransformProvider { get; set; }
        
        internal Call(VersionedName methodGeneratorName)
        {
            _methodGeneratorName = methodGeneratorName;            
        }

        public override void Execute(AnalyzingContext context)
        {
            var generator = context.GetGenerator(_methodGeneratorName);
            context.FetchCallInstructions(_methodGeneratorName,generator);            
        }

        public override string ToString()
        {
            return string.Format("call {0}", _methodGeneratorName);
        }
    }
}
