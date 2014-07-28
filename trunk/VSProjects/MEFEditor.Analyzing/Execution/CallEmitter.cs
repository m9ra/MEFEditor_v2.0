using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// <see cref="EmitterBase"/> implementation that is used for emitting call instructions.
    /// </summary>
    class CallEmitter : EmitterBase
    {
        /// <summary>
        /// Instructions emitted by this emitter.
        /// </summary>
        readonly List<InstructionBase> _instructions = new List<InstructionBase>();

        /// <summary>
        /// Program that has been emitted. Is filled when instructions are inserted, or on GetEmittedInstructions call.
        /// </summary>
        InstructionBatch _emittedProgram;

        /// <summary>
        /// Types resolved for variables.
        /// </summary>
        readonly Dictionary<VariableName, InstanceInfo> _staticVariableInfo = new Dictionary<VariableName, InstanceInfo>();

        /// <summary>
        /// Defined labels pointing to instruction offset.
        /// </summary>
        readonly Dictionary<string, Label> _labels = new Dictionary<string, Label>();

        /// <summary>
        /// Instruction info for currently emitted block.
        /// </summary>
        private InstructionInfo _currentBlockInfo = new InstructionInfo(null);

        /// <summary>
        /// Group id that is currently used for instructions.
        /// </summary>
        private object _currentGroupID;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitterBase" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        internal CallEmitter(AnalyzingContext context)
            : base(context)
        {
        }

        #region Emitter API implementation

        /// <summary>
        /// Assigns the literal to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="literal">The literal.</param>
        /// <param name="literalInfo">The literal information.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder AssignLiteral(string targetVar, object literal, InstanceInfo literalInfo)
        {
            var literalInstance = Context.Machine.CreateDirectInstance(literal, literalInfo);
            var target = getVariable(targetVar, literalInstance.Info);

            var result = new AssignLiteral(target, literalInstance);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Assigns the instance to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceInfo">The instance information.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder AssignInstance(string targetVar, Instance instance, InstanceInfo instanceInfo = null)
        {
            if (instanceInfo == null)
                instanceInfo = instance.Info;

            var target = getVariable(targetVar, instanceInfo);

            var result = new AssignLiteral(target, instance);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Assigns the specified target variable by value from source variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="sourceVar">The source variable.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder Assign(string targetVar, string sourceVar)
        {
            var source = getVariable(sourceVar);
            var target = getVariable(targetVar, variableInfo(source));

            var result = new Assign(target, source);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Assigns the argument at specified position to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="staticInfo">The static information.</param>
        /// <param name="argumentPosition">The argument position.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder AssignArgument(string targetVar, InstanceInfo staticInfo, uint argumentPosition)
        {
            var target = getVariable(targetVar, staticInfo);

            var result = new AssignArgument(target, argumentPosition);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Assigns the return value.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="returnInfo">The return information.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder AssignReturnValue(string targetVar, InstanceInfo returnInfo)
        {
            var target = getVariable(targetVar, returnInfo);

            var result = new AssignReturnValue(target);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Assigns the new object to target variable.
        /// </summary>
        /// <param name="targetVar">The target variable.</param>
        /// <param name="objectInfo">The object information.</param>
        /// <returns>AssignBuilder.</returns>
        public override AssignBuilder AssignNewObject(string targetVar, InstanceInfo objectInfo)
        {
            var target = getVariable(targetVar, objectInfo);

            var result = new AssignNewObject(target, objectInfo);
            emitInstruction(result);

            return new AssignBuilder(result);
        }

        /// <summary>
        /// Statics the call.
        /// </summary>
        /// <param name="sharedInstanceInfo">The shared instance information.</param>
        /// <param name="methodID">The method identifier.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>CallBuilder.</returns>
        /// <exception cref="System.NotSupportedException">Initializers doesn't support dynamic resolving</exception>
        public override CallBuilder StaticCall(InstanceInfo sharedInstanceInfo, MethodID methodID, Arguments arguments)
        {
            var sharedThisVar = getSharedVar(sharedInstanceInfo);

            var initializerID = Context.Settings.GetSharedInitializer(sharedInstanceInfo);

            if (initializerID != null && initializerID.NeedsDynamicResolving)
            {
                throw new NotSupportedException("Initializers doesn't support dynamic resolving");
            }

            var ensureInitialization = new EnsureInitialized(sharedThisVar, sharedInstanceInfo, initializerID);

            arguments.Initialize(sharedThisVar);


            emitInstruction(ensureInitialization);

            return emitCall(methodID, arguments);
        }

        /// <summary>
        /// Calls the specified method identifier.
        /// </summary>
        /// <param name="methodID">The method identifier.</param>
        /// <param name="thisObjVariable">The this object variable.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>CallBuilder.</returns>
        public override CallBuilder Call(MethodID methodID, string thisObjVariable, Arguments arguments)
        {
            var thisVar = getVariable(thisObjVariable);
            var thisType = variableInfo(thisVar);

            arguments.Initialize(thisVar);

            return emitCall(methodID, arguments);
        }

        /// <summary>
        /// Emit direct invocation of given native method.
        /// </summary>
        /// <param name="method">The method.</param>
        public override void DirectInvoke(DirectMethod method)
        {
            emitInstruction(new DirectInvoke(method));
        }

        /// <summary>
        /// Emit return which finishes call and set return value stored in specified variable.
        /// </summary>
        /// <param name="sourceVar">The variable with return value.</param>
        public override void Return(string sourceVar)
        {
            var sourceVariable = getVariable(sourceVar);
            emitInstruction(new Return(sourceVariable));
        }

        /// <summary>
        /// Creates label.
        /// NOTE:
        /// Every label has to be initialized by SetLabel.
        /// </summary>
        /// <param name="identifier">Label identifier.</param>
        /// <returns>Created label.</returns>
        public override Label CreateLabel(string identifier)
        {
            var label = new Label(identifier);

            _labels.Add(label.LabelName, label);

            return label;
        }

        /// <summary>
        /// Set label pointing to next instruction that will be generated.
        /// </summary>
        /// <param name="label">Label that will be set.</param>
        /// <exception cref="System.NotSupportedException">This label cannot be set by this emitter</exception>
        public override void SetLabel(Label label)
        {
            if (!_labels.ContainsKey(label.LabelName) || _labels[label.LabelName] != label)
            {
                throw new NotSupportedException("This label cannot be set by this emitter");
            }

            label.SetOffset((uint)_instructions.Count);
        }

        /// <summary>
        /// Jumps at given target if instance under conditionVariable is resolved as true.
        /// </summary>
        /// <param name="conditionVariable">Variable where condition is stored.</param>
        /// <param name="target">Target label.</param>
        public override void ConditionalJump(string conditionVariable, Label target)
        {
            var condition = getVariable(conditionVariable);
            var conditionalJump = new ConditionalJump(condition, target);

            emitInstruction(conditionalJump);
        }

        /// <summary>
        /// Jumps at given target.
        /// </summary>
        /// <param name="target">Target label.</param>
        public override void Jump(Label target)
        {
            var jump = new Jump(target);
            emitInstruction(jump);
        }

        /// <summary>
        /// Emit no-operation instruction (nop).
        /// </summary>
        public override void Nop()
        {
            emitInstruction(new Nop());
        }

        /// <summary>
        /// Create new instruction info for block starting with next emitted instruction.
        /// </summary>
        /// <returns>Created instruction info.</returns>
        public override InstructionInfo StartNewInfoBlock()
        {
            _currentBlockInfo = new InstructionInfo(_currentGroupID);
            return _currentBlockInfo;
        }

        /// <summary>
        /// Sets the current group.
        /// </summary>
        /// <param name="groupID">The group identifier.</param>
        public override void SetCurrentGroup(object groupID)
        {
            _currentGroupID = groupID;
        }


        /// <summary>
        /// Get variable, which is not used yet in emitted code.
        /// </summary>
        /// <param name="description">Description of variable, used in name.</param>
        /// <returns>Name of variable.</returns>
        public override string GetTemporaryVariable(string description = "")
        {
            var variable = "$tmp" + description;
            var index = _staticVariableInfo.Count;
            VariableName toRegister;
            while (_staticVariableInfo.ContainsKey(toRegister = new VariableName(variable + index)))
            {
                ++index;
            }

            _staticVariableInfo.Add(toRegister, default(InstanceInfo));
            return variable + index;
        }

        /// <summary>
        /// Get label, which is not used yet in emitted code.
        /// </summary>
        /// <param name="description">Description of label, used in name.</param>
        /// <returns>Created label.</returns>
        public override Label GetTemporaryLabel(string description = "")
        {
            string labelName;
            var index = _labels.Count;

            while (_labels.ContainsKey(labelName = "$lbl" + index + description))
            {
                ++index;
            }

            return CreateLabel(labelName);
        }

        /// <summary>
        /// Returns instance info stored for given variable.
        /// </summary>
        /// <param name="variable">Variable which info is resolved.</param>
        /// <returns>Stored info.</returns>
        public override InstanceInfo VariableInfo(string variable)
        {
            return variableInfo(getVariable(variable));
        }

        #endregion

        #region Internal emittor services

        /// <summary>
        /// Get emitted program.
        /// </summary>
        /// <returns>Program that has been emitted.</returns>
        public override InstructionBatch GetEmittedInstructions()
        {
            if (_emittedProgram == null)
                _emittedProgram = new InstructionBatch(_instructions.ToArray());

            return _emittedProgram;
        }

        /// <summary>
        /// Insert given batch of instructions (as they were emitted).
        /// </summary>
        /// <param name="instructions">Inserted instructions.</param>
        /// <exception cref="System.NotSupportedException">Cannot insert instructions twice.</exception>
        public override void InsertInstructions(InstructionBatch instructions)
        {
            if (_emittedProgram != null)
                throw new NotSupportedException("Cannot insert instructions twice.");
            _emittedProgram = instructions;
        }

        /// <summary>
        /// Add given instruction into generated program
        /// <remarks>
        /// All instructions are assigned into current instruction block
        /// </remarks>.
        /// </summary>
        /// <param name="instruction">Added instruction.</param>
        private void emitInstruction(InstructionBase instruction)
        {
            instruction.Info = _currentBlockInfo;
            _instructions.Add(instruction);
        }

        /// <summary>
        /// Emits the call.
        /// </summary>
        /// <param name="methodID">The method identifier.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>CallBuilder.</returns>
        private CallBuilder emitCall(MethodID methodID, Arguments arguments)
        {
            var call = new Call(methodID, arguments);
            emitInstruction(call);

            var builder = new CallBuilder(call);
            return builder;
        }

        #endregion

        #region Private variable services

        /// <summary>
        /// Gets the shared variable.
        /// </summary>
        /// <param name="sharedInstanceInfo">The shared instance information.</param>
        /// <returns>VariableName.</returns>
        private VariableName getSharedVar(InstanceInfo sharedInstanceInfo)
        {
            return getVariable("#shared_" + sharedInstanceInfo.TypeName, sharedInstanceInfo);
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="type">The type.</param>
        /// <returns>VariableName.</returns>
        private VariableName getVariable(string variable, Type type = null)
        {
            InstanceInfo info = default(InstanceInfo);

            if (type != null)
            {
                info = Context.Settings.GetNativeInfo(type);
            }

            return getVariable(variable, info);
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="info">The information.</param>
        /// <returns>VariableName.</returns>
        private VariableName getVariable(string variable, InstanceInfo info)
        {
            var variableName = new VariableName(variable);

            InstanceInfo currentInfo;

            if (info != null && (!_staticVariableInfo.TryGetValue(variableName, out currentInfo) || object.Equals(currentInfo, null)))
            {
                //firstly determined variable info
                _staticVariableInfo[variableName] = info;
            }

            return variableName;
        }

        /// <summary>
        /// Variables the information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>InstanceInfo.</returns>
        private InstanceInfo variableInfo(VariableName name)
        {
            InstanceInfo varType;
            _staticVariableInfo.TryGetValue(name, out varType);
            return varType;
        }

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <returns>InstanceInfo[].</returns>
        private InstanceInfo[] getInfo(VariableName[] variables)
        {
            var result = new InstanceInfo[variables.Length];

            for (int i = 0; i < variables.Length; ++i)
            {
                result[i] = variableInfo(variables[i]);
            }

            return result;
        }

        #endregion


    }
}
