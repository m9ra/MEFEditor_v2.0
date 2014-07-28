using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// Representation of call context of call stack of <see cref="Machine"/>.
    /// </summary>
    public class CallContext
    {
        /// <summary>
        /// The declared variables.
        /// </summary>
        private readonly Dictionary<VariableName, Instance> _variables = new Dictionary<VariableName, Instance>();

        /// <summary>
        /// Pointer to currently processed instruction.
        /// </summary>
        private uint _instructionPointer;

        /// <summary>
        /// Current analyzing context.
        /// </summary>
        private readonly AnalyzingContext _context;

        /// <summary>
        /// Is used by analyzing context. All chained dynamic calls are executed after current call is popped.
        /// Nested dynamic calls are chained separately.
        /// </summary>
        internal DynamicCallEntry ContextsDynamicCalls;

        /// <summary>
        /// Dynamic calls that are not chained via this call. They are executed after current call is popped.
        /// Is used for context calls chaining separation.
        /// </summary>
        internal DynamicCallEntry FollowingDynamicCalls;

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed.
        /// </summary>
        /// <value><c>true</c> if this instance is call end; otherwise, <c>false</c>.</value>
        internal bool IsCallEnd { get { return _instructionPointer >= Program.Instructions.Length; } }

        /// <summary>
        /// Gets argument values of represented call.
        /// </summary>
        /// <value>The argument values.</value>
        internal Instance[] ArgumentValues { get; private set; }

        /// <summary>
        /// The transform provider.
        /// </summary>
        internal readonly CallTransformProvider TransformProvider;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<VariableName> Variables { get { return _variables.Keys; } }

        /// <summary>
        /// Gets the current block.
        /// </summary>
        /// <value>The current block.</value>
        public ExecutedBlock CurrentBlock { get; private set; }

        /// <summary>
        /// The entry block.
        /// </summary>
        public readonly ExecutedBlock EntryBlock;

        /// <summary>
        /// Block from which current call was called.
        /// </summary>
        public readonly ExecutedBlock CallingBlock;

        /// <summary>
        /// The name of called method.
        /// </summary>
        public readonly MethodID Name;

        /// <summary>
        /// Instructions generated for call.
        /// </summary>
        public readonly InstructionBatch Program;

        /// <summary>
        /// Call which invokes current call if available, <c>null</c> otherwise.
        /// </summary>
        public readonly CallContext Caller;


        /// <summary>
        /// Initializes a new instance of the <see cref="CallContext" /> class.
        /// </summary>
        /// <param name="context">Current analyzing context.</param>
        /// <param name="name">The name.</param>
        /// <param name="transformProvider">The transform provider.</param>
        /// <param name="generator">The generator of call's instructions.</param>
        /// <param name="argumentValues">The argument values.</param>
        internal CallContext(AnalyzingContext context, MethodID name, CallTransformProvider transformProvider, GeneratorBase generator, Instance[] argumentValues)
        {
            _context = context;

            Caller = context.CurrentCall;
            ArgumentValues = argumentValues;
            Name = name;
            TransformProvider = transformProvider;

            if (Caller != null)
                CallingBlock = Caller.CurrentBlock;

            var emitter = new CallEmitter(context);
            generator.Generate(emitter);

            Program = emitter.GetEmittedInstructions();
            _instructionPointer = 0;

            if (Program.Instructions.Length <= _instructionPointer)
                //cannot run empty program
                return;

            EntryBlock = new ExecutedBlock(Program.Instructions[0].Info, this);
            CurrentBlock = EntryBlock;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="value">The value.</param>
        internal void SetValue(VariableName targetVariable, Instance value)
        {
            if (value == null)
            {
                value = _context.Machine.Null;
            }

            Instance oldInstance;
            _variables.TryGetValue(targetVariable, out oldInstance);
            _variables[targetVariable] = value;

            var assignInstruction = Program.Instructions[_instructionPointer - 1] as AssignBase;

            CurrentBlock.RegisterAssign(targetVariable, assignInstruction, oldInstance, value);
        }

        /// <summary>
        /// Gets the value of given variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>Instance.</returns>
        public Instance GetValue(VariableName variable)
        {
            Instance value;

            if (!_variables.TryGetValue(variable, out value))
            {
                return _context.GetGlobal(variable);
            }

            return value;
        }
        
        /// <summary>
        /// Determines whether variable of given name is defined.
        /// </summary>
        /// <param name="name">The name of variable.</param>
        /// <returns><c>true</c> if variable is defined; otherwise, <c>false</c>.</returns>
        public bool IsVariableDefined(string name)
        {
            var variable = new VariableName(name);
            return _variables.ContainsKey(variable);
        }

        /// <summary>
        /// Get next instruction according to current value of instruction pointer.
        /// </summary>
        /// <returns>InstructionBase.</returns>
        internal InstructionBase NextInstrution()
        {
            if (IsCallEnd)
            {
                return null;
            }

            var instruction = Program.Instructions[_instructionPointer++];
            if (instruction.Info != CurrentBlock.Info)
                setNewBlock(instruction);

            return instruction;
        }

        /// <summary>
        /// Determines whether specified variable is defined.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <returns><c>true</c> if variable is defined; otherwise, <c>false</c>.</returns>
        public bool Contains(VariableName targetVariable)
        {
            return _variables.ContainsKey(targetVariable);
        }

        /// <summary>
        /// Jumps to the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        internal void Jump(Label target)
        {
            _instructionPointer = target.InstructionOffset;
        }

        /// <summary>
        /// Registers the given call.
        /// </summary>
        /// <param name="call">The call.</param>
        internal void RegisterCall(CallContext call)
        {
            CurrentBlock.RegisterCall(call);
        }

        /// <summary>
        /// Creates new instruction block for given instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        private void setNewBlock(InstructionBase instruction)
        {
            var newExecutedBlock = new ExecutedBlock(instruction.Info, this);
            CurrentBlock.NextBlock = newExecutedBlock;
            CurrentBlock = newExecutedBlock;
        }

    }
}
