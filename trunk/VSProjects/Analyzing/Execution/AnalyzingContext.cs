using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class AnalyzingContext
    {
        /// <summary>
        /// Current call stack
        /// </summary>
        Stack<CallContext> _callStack = new Stack<CallContext>();

        /// <summary>
        /// Current call context on call stack
        /// </summary>
        private CallContext CurrentCall { get { return _callStack.Peek(); } }

        /// <summary>
        /// Determine that execution has ended now
        /// </summary>
        public bool IsExecutionEnd { get; private set; }


        /// <summary>
        /// Get current instance stored in variable of given name
        /// </summary>
        /// <param name="variable">Name of variable</param>
        /// <returns>Stored instance</returns>
        internal Instance GetValue(VariableName variable)
        {
            return CurrentCall.GetValue(variable);
        }
        /// <summary>
        /// Set value for variable of given name
        /// </summary>
        /// <param name="targetVaraiable">Name of variable</param>
        /// <param name="value">Value that will be set to variable</param>
        internal void SetValue(VariableName targetVaraiable, Instance value)
        {
            CurrentCall.SetValue(targetVaraiable, value);
        }

        /// <summary>
        /// Fetch instructions from given generator
        /// <param name="arguments">Names of variables where arguments are stored</param>
        /// </summary>
        /// <param name="generator">Generator of fetched instructions</param>
        internal void FetchCallInstructions(IInstructionGenerator generator, params VariableName[] arguments)
        {
            var argumentValues = getArgumentValues(arguments);
            var call = new CallContext(generator, argumentValues);
            _callStack.Push(call);
        }

        /// <summary>
        /// Get next available instrution
        /// </summary>
        /// <returns>Instruction that is on turn to be processed, if end of execution returns null</returns>
        internal IInstruction NextInstruction()
        {
            IInstruction instrution;
            while((instrution=CurrentCall.NextInstrution())==null){
                _callStack.Pop();

                if (_callStack.Count == 0)
                {
                    IsExecutionEnd = true;
                    return null;
                }                
            }

            return instrution;
        }

        /// <summary>
        /// Get generator for given name
        /// </summary>
        /// <param name="methodName">Name of method generator</param>
        /// <returns>Instruction generator for given name</returns>
        internal IInstructionGenerator GetGenerator(VersionedName methodName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get current result of analysis
        /// </summary>
        /// <returns>Result of analysis</returns>
        internal AnalyzingResult GetResult()
        {
            return new AnalyzingResult();
        }

        /// <summary>
        /// Return argument values for given argument variable names
        /// </summary>
        /// <param name="arguments">Names of argument variables where values are stored</param>
        /// <returns>Argument values</returns>
        private Instance[] getArgumentValues(VariableName[] arguments)
        {
            var values = new List<Instance>();
            foreach (var argument in arguments)
            {
                values.Add(GetValue(argument));
            }

            return values.ToArray();
        }
    }
}
