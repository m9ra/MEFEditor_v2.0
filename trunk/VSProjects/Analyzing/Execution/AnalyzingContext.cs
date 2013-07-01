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
        /// Loader used for loading and resolving methods and type descriptions
        /// </summary>
        private readonly IInstructionLoader _loader;
        /// <summary>
        /// Execution entry context
        /// </summary>
        private CallContext _entryContext;
        /// <summary>
        /// Current call context on call stack
        /// </summary>
        private CallContext CurrentCall { get { return _callStack.Peek(); } }

        /// <summary>
        /// Arguments prepared for call invoking
        /// </summary>
        private VariableName[] _preparedArguments = null;


        internal Instance[] CurrentArguments { get { return CurrentCall.ArgumentValues; } }
        /// <summary>
        /// Determine that execution has ended now
        /// </summary>
        internal bool IsExecutionEnd { get; private set; }

        /// <summary>
        /// Return value of lastly proceeded call
        /// </summary>
        internal Instance LastReturnValue { get; private set; }



        internal AnalyzingContext(IInstructionLoader loader)
        {
            _loader = loader;
        }

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

        internal void PrepareCall(params VariableName[] arguments)
        {
            _preparedArguments = arguments;
        }

        /// <summary>
        /// Fetch instructions from given generator
        /// <param name="arguments">Names of variables where arguments are stored</param>
        /// </summary>
        /// <param name="generator">Generator of fetched instructions</param>
        internal void FetchCallInstructions(IInstructionGenerator generator)
        {
            var argumentValues = getArgumentValues(_preparedArguments);
            //preparing is just for single call
            _preparedArguments = null;
            var call = new CallContext(_loader,generator, argumentValues);
            if (_entryContext == null)
            {
                _entryContext = call;   
            }
            _callStack.Push(call);
        }

        /// <summary>
        /// Get next available instrution
        /// </summary>
        /// <returns>Instruction that is on turn to be processed, if end of execution returns null</returns>
        internal IInstruction NextInstruction()
        {
            IInstruction instrution=null;
            while(!IsExecutionEnd && (instrution=CurrentCall.NextInstrution())==null){
                popContext();                
            }

            return instrution;
        }

        private void popContext()
        {
            var poppedContext = _callStack.Pop();

            if (_callStack.Count == 0)
            {
                IsExecutionEnd = true;                
            }
        }

        /// <summary>
        /// Get generator for given name
        /// </summary>
        /// <param name="methodName">Name of method generator</param>
        /// <returns>Instruction generator for given name</returns>
        internal IInstructionGenerator GetGenerator(VersionedName methodName)
        {
            return _loader.GetGenerator(methodName);
        }

        /// <summary>
        /// Get current result of analysis
        /// </summary>
        /// <returns>Result of analysis</returns>
        internal AnalyzingResult GetResult()
        {
            return new AnalyzingResult(_entryContext);
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

        internal void Return(Instance returnValue)
        {
            popContext();
            LastReturnValue = returnValue;                        
        }

        internal bool Contains(VariableName targetVariable)
        {
            return CurrentCall.Contains(targetVariable);
        }




    }
}
