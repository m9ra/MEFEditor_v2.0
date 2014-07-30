using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Instruction for ensuring global variable initialization.
    /// </summary>
    class EnsureInitialized : InstructionBase
    {
        /// <summary>
        /// The target variable.
        /// </summary>
        private readonly VariableName _targetVariable;

        /// <summary>
        /// The global variable initializer.
        /// </summary>
        private readonly MethodID _initializer;

        /// <summary>
        /// The shared instance information.
        /// </summary>
        private readonly InstanceInfo _sharedInstanceInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureInitialized" /> class.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="sharedInstanceInfo">The shared instance information.</param>
        /// <param name="initializator">The initializator.</param>
        internal EnsureInitialized(VariableName targetVariable, InstanceInfo sharedInstanceInfo, MethodID initializator)
        {
            _targetVariable = targetVariable;
            _initializer = initializator;
            _sharedInstanceInfo = sharedInstanceInfo;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var initializer = _initializer == null ? "`nothing`" : _initializer.ToString();
            return string.Format("ensure_init {0} by {1}", _targetVariable, initializer);
        }
    }
}
