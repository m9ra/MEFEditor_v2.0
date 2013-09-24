using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
{
    class EnsureInitialized : InstructionBase
    {
        private readonly VariableName _targetVariable;
        private readonly MethodID _initializator;
        private readonly InstanceInfo _sharedInstanceInfo;

        internal EnsureInitialized(VariableName targetVariable, InstanceInfo sharedInstanceInfo, MethodID initializator)
        {
            _targetVariable = targetVariable;
            _initializator = initializator;
            _sharedInstanceInfo = sharedInstanceInfo;
        }

        public override void Execute(AnalyzingContext context)
        {
            if (context.Contains(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //create shared instance
            var sharedInstance = context.Machine.CreateInstance(_sharedInstanceInfo);
            context.SetValue(_targetVariable, sharedInstance);

            //run initializer generator
            var generator = context.GetGenerator(_initializator);

            context.FetchCallInstructions(_initializator, generator, new Instance[] { sharedInstance });
        }

        public override string ToString()
        {
            return string.Format("ensure_init {0} by {1}", _targetVariable, _initializator);
        }
    }
}
