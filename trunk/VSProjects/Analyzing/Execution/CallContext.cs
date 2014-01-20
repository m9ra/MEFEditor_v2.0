using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    public class CallContext
    {
        private readonly Dictionary<VariableName, Instance> _variables = new Dictionary<VariableName, Instance>();

        private uint _instructionPointer;

        private readonly AnalyzingContext _context;

        /// <summary>
        /// Is used by analyzing context. All chained dynamic calls are executed after current call is popped.
        /// Nested dynamic calls are chained separately
        /// </summary>
        internal DynamicCallEntry ContextsDynamicCalls;

        /// <summary>
        /// Dynamic calls that are not chained via this call. They are executed after current call is popped.
        /// Is used for context calls chaining separation.
        /// </summary>
        internal DynamicCallEntry FollowingDynamicCalls;

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed
        /// </summary>
        internal bool IsCallEnd { get { return _instructionPointer >= Program.Instructions.Length; } }

        internal Instance[] ArgumentValues { get; private set; }

        internal readonly CallTransformProvider TransformProvider;

        public IEnumerable<VariableName> Variables { get { return _variables.Keys; } }

        public ExecutedBlock CurrentBlock { get; private set; }

        public readonly ExecutedBlock EntryBlock;

        public readonly MethodID Name;

        public readonly InstructionBatch Program;

        public readonly CallContext Caller;


        internal CallContext(AnalyzingContext context, MethodID name, CallTransformProvider transformProvider, GeneratorBase generator, Instance[] argumentValues)
        {
            _context = context;

            Caller = context.CurrentCall;
            ArgumentValues = argumentValues;
            Name = name;
            TransformProvider = transformProvider;

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

        internal void SetValue(VariableName targetVaraiable, Instance value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value cannot be null");
            }

            Instance oldInstance;
            _variables.TryGetValue(targetVaraiable, out oldInstance);
            _variables[targetVaraiable] = value;

            var assignInstruction = Program.Instructions[_instructionPointer - 1] as AssignBase;

            CurrentBlock.RegisterAssign(targetVaraiable, assignInstruction, oldInstance, value);
        }

        public Instance GetValue(VariableName variable)
        {
            Instance value;

            if (!_variables.TryGetValue(variable, out value))
            {
                return _context.GetGlobal(variable);
            }

            return value;
        }


        public bool IsVariableDefined(string name)
        {
            var variable=new VariableName(name);
            return _variables.ContainsKey(variable);
        }

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

        public bool Contains(VariableName targetVariable)
        {
            return _variables.ContainsKey(targetVariable);
        }

        internal void Jump(Label target)
        {
            _instructionPointer = target.InstructionOffset;
        }

        internal void RegisterCall(CallContext call)
        {
            CurrentBlock.RegisterCall(call);
        }

        private void setNewBlock(InstructionBase instruction)
        {
            var newExecutedBlock = new ExecutedBlock(instruction.Info, this);
            CurrentBlock.NextBlock = newExecutedBlock;
            CurrentBlock = newExecutedBlock;
        }

    }
}
