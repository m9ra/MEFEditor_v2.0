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
        /// Current call stack
        /// </summary>
        private readonly Stack<CallContext> _callStack = new Stack<CallContext>();

        /// <summary>
        /// Variables that are available globaly through call stack
        /// </summary>
        private readonly Dictionary<VariableName, Instance> _globals = new Dictionary<VariableName, Instance>();

        /// <summary>
        /// Resolved methods - it is needed because of avoiding inconsistent resolvings
        /// </summary>
        private readonly Dictionary<MethodID, GeneratorBase> _methods = new Dictionary<MethodID, GeneratorBase>();

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
        public CallContext CurrentCall
        {
            get
            {
                if (_callStack.Count == 0)
                {
                    return null;
                }
                else
                {
                    return _callStack.Peek();
                }
            }
        }

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
        /// Available machine settings
        /// </summary>
        internal MachineSettingsBase Settings { get { return Machine.Settings; } }

        public readonly Machine Machine;

        /// <summary>
        /// Provider for Edits handling
        /// </summary>
        public EditsProvider Edits { get; private set; }

        internal AnalyzingContext(Machine machine, LoaderBase loader)
        {
            Machine = machine;
            _loader = loader;
        }

        /// <summary>
        /// Get current instance stored in variable of given name
        /// </summary>
        /// <param name="variable">Name of variable</param>
        /// <returns>Stored instance</returns>
        public Instance GetValue(VariableName variable)
        {
            return CurrentCall.GetValue(variable);
        }

        /// <summary>
        /// Determine that gloabl scope contains given variable
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        internal bool ContainsGlobal(VariableName variable)
        {
            return _globals.ContainsKey(variable);
        }

        internal Instance GetGlobal(VariableName variable)
        {
            Instance result;

            if (!_globals.TryGetValue(variable, out result))
            {
                throw new KeyNotFoundException("Cannot find " + variable + " in global scope");
            }

            return result;
        }

        internal void SetGlobal(VariableName variable, Instance instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            _globals[variable] = instance;
        }

        /// <summary>
        /// Set value for variable of given name
        /// </summary>
        /// <param name="targetVaraiable">Name of variable</param>
        /// <param name="value">Value that will be set to variable</param>
        public void SetValue(VariableName targetVaraiable, Instance value)
        {
            value.HintID(targetVaraiable.Name, this);
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

        /// <summary>
        /// Fetch instructions from given generator
        /// <param name="arguments">Names of variables where arguments are stored</param>
        /// </summary>
        /// <param name="generator">Generator of fetched instructions</param>
        internal void FetchCall(MethodID name, Instance[] argumentValues)
        {
            var generator = resolveGenerator(ref name, argumentValues);

            PushCall(name, generator, argumentValues);
        }

        private GeneratorBase resolveGenerator(ref MethodID name, Instance[] argumentValues)
        {
            var overridingGenerator = _loader.GetOverridingGenerator(name, argumentValues);
            if (overridingGenerator != null)
                //notice that generator is not cached
                //for resolving name
                return overridingGenerator;

            InstanceInfo[] dynamicInfo = null;
            if (name.NeedsDynamicResolving)
            {
                dynamicInfo = new InstanceInfo[argumentValues.Length];
                for (int i = 0; i < dynamicInfo.Length; ++i)
                {
                    dynamicInfo[i] = argumentValues[i].Info;
                }
            }

            var generator = getGenerator(ref name, dynamicInfo);
            return generator;
        }


        public void DynamicCall(string methodNameHint, GeneratorBase generator, params Instance[] argumentValues)
        {
            var dynamicCall = new DynamicCallEntry(new MethodID(methodNameHint, false), generator, argumentValues.ToArray());

            if (CurrentCall.ContextsDynamicCalls == null)
            {
                CurrentCall.ContextsDynamicCalls = dynamicCall;
            }
            else
            {
                CurrentCall.ContextsDynamicCalls.LastCall.NextDynamicCall = dynamicCall;
            }
        }

        internal void PushCall(MethodID name, GeneratorBase generator, Instance[] argumentValues)
        {
            var callTransformProvider = Edits == null ? null : Edits.TransformProvider;
            var isDirty = generator == null || argumentValues.Any((value) => value.IsDirty);

            if (isDirty)
            {
                //For dirty call we will propagate dirty flag
                foreach (var argumentValue in argumentValues)
                {
                    argumentValue.IsDirty = true;
                }
                //and call wont be pushed
                return;
            }



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

            handleDynamicCalls(poppedContext);

            if (_callStack.Count == 0)
            {
                IsExecutionEnd = true;
            }
        }

        private void handleDynamicCalls(CallContext poppedContext)
        {
            //handle dynamic call chain if needed
            var dynamicCall = poppedContext.ContextsDynamicCalls;
            var enqueueFollowingDynamics = true;
            if (dynamicCall == null)
            {
                //there are no preceding calls generated by poppedContext
                //so we may enqueue following calls generated earlier
                dynamicCall = poppedContext.FollowingDynamicCalls;
                enqueueFollowingDynamics = false;
            }

            if (dynamicCall != null)
            {
                PushCall(dynamicCall.Method, dynamicCall.Generator, dynamicCall.Arguments);
                CurrentCall.FollowingDynamicCalls = dynamicCall.NextDynamicCall;
            }

            if (enqueueFollowingDynamics)
                //otherwise theire already included
                addFollowingDynamicCalls(CurrentCall, poppedContext.FollowingDynamicCalls);
        }

        private void addFollowingDynamicCalls(CallContext call, DynamicCallEntry followingCalls)
        {
            var followedCalls = call.FollowingDynamicCalls;
            if (followedCalls == null)
            {
                call.FollowingDynamicCalls = followingCalls;
            }
            else
            {
                followedCalls.LastCall.NextDynamicCall = followingCalls;
            }
        }

        /// <summary>
        /// Get generator for given name
        /// </summary>
        /// <param name="methodName">Name of method generator</param>
        /// <returns>Instruction generator for given name</returns>
        private GeneratorBase getGenerator(ref MethodID method, InstanceInfo[] arguments)
        {
            if (method.NeedsDynamicResolving)
            {
                method = _loader.DynamicResolve(method, arguments);
            }

            if (method == null)
                return null;

            GeneratorBase resolved;
            if (!_methods.TryGetValue(method, out resolved))
            {
                //register resolved method
                resolved = _loader.StaticResolve(method);
                _methods[method] = resolved;
            }

            return resolved;
        }

        /// <summary>
        /// Get current result of analysis
        /// </summary>
        /// <param name="createdInstances">Enumeration of all instances created during execution</param>
        /// <returns>Result of analysis</returns>
        internal AnalyzingResult GetResult(Dictionary<string, Instance> createdInstances)
        {            
            return new AnalyzingResult(LastReturnValue, _entryContext, createdInstances, _methods.Keys);
        }

        /// <summary>
        /// Return argument values for given argument variable names
        /// </summary>
        /// <param name="arguments">Names of argument variables where values are stored</param>
        /// <returns>Argument values</returns>
        internal Instance[] GetArguments(Arguments arguments)
        {
            var values = new List<Instance>();
            foreach (var argument in arguments.ValueVariables)
            {
                values.Add(GetValue(argument));
            }

            return values.ToArray();
        }

        public void Return(Instance returnValue)
        {
            popContext();
            LastReturnValue = returnValue;
            if (LastReturnValue == null)
                //TODO resolve null values same as dirty instances
                LastReturnValue = Machine.CreateDirectInstance("Null");
        }

        public bool Contains(VariableName targetVariable)
        {
            return CurrentCall.Contains(targetVariable);
        }

        public void Initialize(Instance instance, object data)
        {
            var directInstance = instance as DirectInstance;
            directInstance.Initialize(data);
        }

        internal void Jump(Label target)
        {
            CurrentCall.Jump(target);
        }

        internal bool IsTrue(VariableName condition)
        {
            return Settings.IsTrue(GetValue(condition));
        }

        public void ShareEdits(EditsProvider edits)
        {
            Edits = edits;
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
