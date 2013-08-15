using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    class CallEmitter<MethodID, InstanceInfo> : IEmitter<MethodID, InstanceInfo>
    {
        /// <summary>
        /// Owning loader - is used for resolving
        /// </summary>
        readonly IInstructionLoader<MethodID, InstanceInfo> _loader;
        /// <summary>
        /// Available machine settings
        /// </summary>
        readonly IMachineSettings<InstanceInfo> _settings;

        /// <summary>
        /// Instructions emitted by this emitter
        /// </summary>
        readonly List<InstructionBase<MethodID, InstanceInfo>> _instructions = new List<InstructionBase<MethodID, InstanceInfo>>();
        /// <summary>
        /// Types resolved for variables
        /// </summary>
        readonly Dictionary<VariableName, InstanceInfo> _staticVariableInfo = new Dictionary<VariableName, InstanceInfo>();
        /// <summary>
        /// Defined labels pointing to instruction offset
        /// </summary>
        readonly HashSet<Label> _labels = new HashSet<Label>();

        /// <summary>
        /// Instruction info for currently emitted block
        /// </summary>
        private InstructionInfo _currentBlockInfo = new InstructionInfo();


        internal CallEmitter(IMachineSettings<InstanceInfo> settings, IInstructionLoader<MethodID, InstanceInfo> loader)
        {
            _settings = settings;
            _loader = loader;
        }

        #region Emittor API implementation
        public void AssignLiteral(string targetVar, object literal)
        {
            var target = getVariable(targetVar, literal.GetType());
            var literalInstance = new Instance(literal);

            emitInstruction(new AssignLiteral<MethodID, InstanceInfo>(target, literalInstance));
        }

        public void Assign(string targetVar, string sourceVar)
        {
            var source = getVariable(sourceVar);
            var target = getVariable(targetVar, variableInfo(source));

            emitInstruction(new Assign<MethodID, InstanceInfo>(target, source));
        }

        public void AssignReturnValue(string targetVar)
        {
            //TODO resolve return value of previous call
            var target = getVariable(targetVar);

            emitInstruction(new AssignReturnValue<MethodID, InstanceInfo>(target));
        }

        public void StaticCall(string typeFullname,MethodID methodID, params string[] inputVariables)
        {
            var inputArgumentVars = translateVariables(inputVariables);
            var sharedThisVar = getSharedVar(typeFullname);
            var callArgVars = new VariableName[] { sharedThisVar }.Concat(inputArgumentVars).ToArray();

            var generatorName = _loader.ResolveCallName(methodID,getInfo(callArgVars));
            var initializatorName = _loader.ResolveStaticInitializer(variableInfo(sharedThisVar));

            var ensureInitialization = new EnsureInitialized<MethodID,InstanceInfo>(sharedThisVar, initializatorName);
            var lateInitialization = new LateReturnInitialization<MethodID, InstanceInfo>(sharedThisVar);
            var preCall = new PreCall<MethodID, InstanceInfo>(callArgVars);
            var call = new Call<MethodID, InstanceInfo>(generatorName);

            emitInstruction(ensureInitialization);
            emitInstruction(lateInitialization);
            emitInstruction(preCall);
            emitInstruction(call);
        }

        public void Call(MethodID methodID, string thisObjVariable, params string[] inputVariables)
        {
            var thisVar = getVariable(thisObjVariable);
            var thisType = variableInfo(thisVar);

            var inputArgumentVars = translateVariables(inputVariables);
            var callArgVars = new VariableName[] { thisVar }.Concat(inputArgumentVars).ToArray();
            
            var generatorName = _loader.ResolveCallName(methodID, getInfo(callArgVars));

            var preCall = new PreCall<MethodID, InstanceInfo>(callArgVars);
            var call = new Call<MethodID, InstanceInfo>(generatorName);

            emitInstruction(preCall);
            emitInstruction(call);
        }

        
        public void DirectInvoke(DirectMethod<MethodID, InstanceInfo> method)
        {
            emitInstruction(new DirectInvoke<MethodID,InstanceInfo>(method));
        }

        public void Return(string sourceVar)
        {
            var sourceVariable = getVariable(sourceVar);
            emitInstruction(new Return<MethodID, InstanceInfo>(sourceVariable));
        }
        
        public Label CreateLabel(string identifier)
        {
            var label = new Label(identifier);

            _labels.Add(label);

            return label;
        }

        public void SetLabel(Label label)
        {
            if (!_labels.Contains(label))
            {
                throw new NotSupportedException("This label cannot be set by this emitter");
            }

            label.SetOffset((uint)_instructions.Count);
        }

        public void ConditionalJump(string conditionVariable, Label target)
        {
            var condition = getVariable(conditionVariable);
            var conditionalJump = new ConditionalJump<MethodID, InstanceInfo>(condition,target);

            emitInstruction(conditionalJump);
        }

        public void Jump(Label target)
        {
            var jump = new Jump<MethodID,InstanceInfo>(target);
            emitInstruction(jump);
        }

        public InstructionInfo StartNewInfoBlock()
        {
            _currentBlockInfo = new InstructionInfo();
            return _currentBlockInfo;
        }
        #endregion

        #region Internal emittor services

        /// <summary>
        /// Get emitted program
        /// </summary>
        /// <returns>Program that has been emitted</returns>
        internal InstructionBase<MethodID, InstanceInfo>[] GetEmittedInstructions()
        {
            return _instructions.ToArray();
        }
           
        /// <summary>
        /// Add given instruction into generated program
        /// <remarks>
        ///     All instructions are assigned into current instruction block
        /// </remarks>
        /// </summary>
        /// <param name="instruction">Added instruction</param>
        private void emitInstruction(InstructionBase<MethodID, InstanceInfo> instruction)
        {
            instruction.Info = _currentBlockInfo;
            _instructions.Add(instruction);
        }

        #endregion

        #region Private variable services

        private VariableName getSharedVar(string typeFullname)
        {
            var shared=new VariableName("shared_" + typeFullname);

            if (!_staticVariableInfo.ContainsKey(shared))
            {
                _staticVariableInfo[shared] = _settings.GetSharedInstanceInfo(typeFullname);
            }
            return shared;
        }

        private VariableName getVariable(string variable, Type type = null)
        {
            InstanceInfo info = default(InstanceInfo);

            if (type != null)
            {
                info = _settings.GetLiteralInfo(type);
            }

            return getVariable(variable, info);
        }

        private VariableName getVariable(string variable, InstanceInfo info)
        {
            var variableName = new VariableName(variable);
            if (info != null && !_staticVariableInfo.ContainsKey(variableName))
            {
                //firstly determined variable type
                _staticVariableInfo[variableName] = info;
            }

            return variableName;
        }

        private InstanceInfo variableInfo(VariableName name)
        {
            InstanceInfo varType;
            _staticVariableInfo.TryGetValue(name, out varType);
            return varType;
        }

        private InstanceInfo[] getInfo(VariableName[] variables)
        {
            var result = new InstanceInfo[variables.Length];

            for (int i = 0; i < variables.Length; ++i)
            {
                result[i] = variableInfo(variables[i]);
            }

            return result;
        }

        private IEnumerable<VariableName> translateVariables(string[] inputArguments)
        {
            foreach (var arg in inputArguments)
            {
                yield return new VariableName(arg);
            }
        }

        #endregion


        public InstanceInfo VariableInfo(string variable)
        {
            return variableInfo(getVariable(variable));
        }
    }
}
