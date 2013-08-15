using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class EnsureInitialized<MethodID, InstanceInfo> : InstructionBase<MethodID, InstanceInfo>
    {
        private readonly VariableName _targetVariable;
        private readonly VersionedName _initializator;


        internal EnsureInitialized(VariableName targetVariable, VersionedName initializator)
        {
            _targetVariable = targetVariable;
            _initializator = initializator;
        }

        public override void Execute(AnalyzingContext<MethodID, InstanceInfo> context)
        {
            if (context.Contains(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //run initializer generator
            var generator = context.GetGenerator(_initializator);
            context.PrepareCall();
            context.FetchCallInstructions(generator);
            //NOTE: call value is supposed to be assigned by late return initialization
        }

        public override string ToString()
        {
            return string.Format("ensure_init {0} by {1}", _targetVariable, _initializator);
        }
    }
}
