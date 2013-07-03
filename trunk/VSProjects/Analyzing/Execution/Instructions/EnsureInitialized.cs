using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class EnsureInitialized<MethodID, InstanceInfo> : IInstruction<MethodID, InstanceInfo>
    {
        private readonly VariableName _targetVariable;
        private readonly VersionedName _initializator;
            
        
        internal EnsureInitialized(VariableName targetVariable, VersionedName initializator)
        {
            _targetVariable = targetVariable;
            _initializator = initializator;
        }

        public void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            if (context.Contains(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //run shared generator
            var generator=context.GetGenerator(_initializator);
            context.PrepareCall();
            context.FetchCallInstructions(generator);
            //NOTE: call value is supposed to be assigned by late return initialization
        }
    }
}
