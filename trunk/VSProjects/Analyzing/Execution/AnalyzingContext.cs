using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    public class AnalyzingContext
    {
        /// <summary>
        /// Available machine settings
        /// </summary>
        internal IMachineSettings Settings { get { return Machine.Settings; } }

        internal readonly Machine Machine;
        /// <summary>
        /// Current call stack
        /// </summary>
        Stack<CallContext> _callStack = new Stack<CallContext>();
        /// <summary>
        /// Loader used for loading and resolving methods and type descriptions
        /// </summary>
        private readonly LoaderBase _loader;
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

        /// <summary>
        /// Arguments for entry call
        /// </summary>
        private Instance[] _entryArguments = null;

        /// <summary>
        /// Array of arguments available for current call
        /// </summary>
        public Instance[] CurrentArguments { get { return CurrentCall.ArgumentValues; } }
        /// <summary>
        /// Determine that execution has ended now
        /// </summary>
        internal bool IsExecutionEnd { get; private set; }

        /// <summary>
        /// Return value of lastly proceeded call
        /// </summary>
        internal Instance LastReturnValue { get; private set; }

        /// <summary>
        /// Provider for Edits handling
        /// </summary>
        public EditsProvider Edits { get; private set; }

        internal AnalyzingContext(Machine machine, LoaderBase loader, Instance[] arguments)
        {
            Machine = machine;
            _loader = loader;
            _entryArguments = arguments;
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

        public void SetField(Instance obj, string fieldName, object value)
        {
            var dataInstance = obj as DataInstance;
            dataInstance.SetField(fieldName, value);
        }

        public object GetField(Instance obj, string fieldName)
        {
            var dataInstance = obj as DataInstance;
            return dataInstance.GetField(fieldName);
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
        internal void FetchCallInstructions(MethodID name, GeneratorBase generator)
        {
            var argumentValues = getArgumentValues(_preparedArguments);
            //preparing is just for single call
            _preparedArguments = null;

            pushCall(name, generator, argumentValues);
        }


        public void DynamicCall(string methodNameHint, GeneratorBase generator, params Instance[] instances)
        {
            pushCall(new MethodID(methodNameHint, false), generator, instances);
        }

        private void pushCall(MethodID name, GeneratorBase generator, Instance[] argumentValues)
        {
            var callTransformProvider = Edits == null ? null : Edits.TransformProvider;
            var call = new CallContext(this, name, callTransformProvider, generator, argumentValues);

            if (_entryContext == null)
            {
                _entryContext = call;
            }
            else
            {
                CurrentCall.RegisterCall(call);
            }

            _callStack.Push(call);
        }

        /// <summary>
        /// Get next available instrution
        /// </summary>
        /// <returns>Instruction that is on turn to be processed, if end of execution returns null</returns>
        internal InstructionBase NextInstruction()
        {
            InstructionBase instrution = null;
            while (!IsExecutionEnd && (instrution = CurrentCall.NextInstrution()) == null)
            {
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
        internal GeneratorBase GetGenerator(MethodID method, params InstanceInfo[] arguments)
        {
            if (method.NeedsDynamicResolving)
            {
                return _loader.DynamicResolve(method, arguments);
            }
            else
            {
                return _loader.StaticResolve(method);
            }
        }

        /// <summary>
        /// Get current result of analysis
        /// </summary>
        /// <returns>Result of analysis</returns>
        internal AnalyzingResult GetResult()
        {
            var removeProvider = new InstanceRemoveProvider(_entryContext);
            return new AnalyzingResult(_entryContext, removeProvider.Remove);
        }

        /// <summary>
        /// Return argument values for given argument variable names
        /// </summary>
        /// <param name="arguments">Names of argument variables where values are stored</param>
        /// <returns>Argument values</returns>
        private Instance[] getArgumentValues(VariableName[] arguments)
        {
            var isEntryContext = _callStack.Count == 0;
            if (isEntryContext)
            {
                return _entryArguments;
            }

            var values = new List<Instance>();
            foreach (var argument in arguments)
            {
                values.Add(GetValue(argument));
            }

            return values.ToArray();
        }

        public void Return(Instance returnValue)
        {
            popContext();
            LastReturnValue = returnValue;
        }

        public bool Contains(VariableName targetVariable)
        {
            return CurrentCall.Contains(targetVariable);
        }

        public Instance CreateDirectInstance(object data, Type type = null)
        {
            if (type == null)
            {
                type = data.GetType();
            }

            var info = Settings.GetNativeInfo(type);
            return CreateDirectInstance(data, info);
        }

        public Instance CreateDirectInstance(object data, InstanceInfo info)
        {
            return Machine.CreateDirectInstance(data, info);
        }

        public Instance CreateInstance(InstanceInfo info)
        {
            return Machine.CreateDataInstance(info);
        }

        internal void Jump(Label target)
        {
            CurrentCall.Jump(target);
        }

        internal bool IsTrue(VariableName condition)
        {
            return Settings.IsTrue(GetValue(condition));
        }

        internal void Prepare(InstructionBase instruction)
        {
            var call = instruction as Call;
            if (call == null)
            {
                //only calls will have edits provider
                if (!(instruction is DirectInvoke))
                    //Direct invoke shares edits provider (because we want to get edits on call place)
                    Edits = null;
            }
            else
            {
                Edits = new EditsProvider(call.TransformProvider, CurrentCall.CurrentBlock);
            }
        }


    }
}
