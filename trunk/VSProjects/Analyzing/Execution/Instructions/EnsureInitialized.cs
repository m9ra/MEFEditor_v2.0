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
        private readonly MethodID _initializer;
        private readonly InstanceInfo _sharedInstanceInfo;

        internal EnsureInitialized(VariableName targetVariable, InstanceInfo sharedInstanceInfo, MethodID initializator)
        {
            _targetVariable = targetVariable;
            _initializer = initializator;
            _sharedInstanceInfo = sharedInstanceInfo;
        }

        public override void Execute(AnalyzingContext context)
        {
            if (context.ContainsGlobal(_targetVariable))
            {
                //shared value is already initialized
                return;
            }

            //create shared instance
            var sharedInstance = context.Machine.CreateInstance(_sharedInstanceInfo);
            context.SetGlobal(_targetVariable, sharedInstance);
            if (_initializer != null)
                context.FetchCall(_initializer, new Instance[] { sharedInstance });
        }

        public override string ToString()
        {
            var initializer = _initializer == null ? "`nothing`" : _initializer.ToString();
            return string.Format("ensure_init {0} by {1}", _targetVariable, initializer);
        }
    }
}
