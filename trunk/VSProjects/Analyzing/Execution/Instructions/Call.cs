using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class Call<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {
        private readonly VersionedName _methodGeneratorName;        

        internal Call(VersionedName methodGeneratorName)
        {
            _methodGeneratorName = methodGeneratorName;            
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            var generator = context.GetGenerator(_methodGeneratorName);
            context.FetchCallInstructions(generator);
        }
    }
}
