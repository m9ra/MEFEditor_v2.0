using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    public class CallContext<MethodID, InstanceInfo>
    {
        private readonly Dictionary<VariableName, Instance> _variables = new Dictionary<VariableName, Instance>();

        private uint _instructionPointer;

        

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed
        /// </summary>
        internal bool IsCallEnd { get { return _instructionPointer >= Program.Instructions.Length; } }

        internal Instance[] ArgumentValues { get; private set; }

        public IEnumerable<VariableName> Variables { get { return _variables.Keys; } }

        public ExecutedBlock<MethodID, InstanceInfo> CurrentBlock { get; private set; }

        public readonly ExecutedBlock<MethodID, InstanceInfo> EntryBlock;

        public readonly VersionedName Name;

        public readonly InstructionBatch<MethodID, InstanceInfo> Program;

        




        internal CallContext(IMachineSettings<InstanceInfo> settings, LoaderBase<MethodID, InstanceInfo> loader, VersionedName name, GeneratorBase<MethodID, InstanceInfo> generator, Instance[] argumentValues)
        {
            ArgumentValues = argumentValues;
            Name = name;

            var emitter = new CallEmitter<MethodID, InstanceInfo>(settings, loader);
            generator.Generate(emitter);

            Program = emitter.GetEmittedInstructions();
            _instructionPointer = 0;

            EntryBlock = new ExecutedBlock<MethodID, InstanceInfo>(Program.Instructions[0].Info,this);
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

            CurrentBlock.RegisterAssign(targetVaraiable, oldInstance, value);
        }

        public Instance GetValue(VariableName variable)
        {
            Instance value;

            if (!_variables.TryGetValue(variable, out value))
            {
                throw new NotSupportedException(string.Format("Missing entry for reading value from '{0}'", variable));
            }

            return value;
        }

        internal InstructionBase<MethodID, InstanceInfo> NextInstrution()
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

        internal void RegisterCall(CallContext<MethodID, InstanceInfo> call)
        {
            CurrentBlock.RegisterCall(call);
        }

        private void setNewBlock(InstructionBase<MethodID, InstanceInfo> instruction)
        {
            var newExecutedBlock = new ExecutedBlock<MethodID, InstanceInfo>(instruction.Info,this);
            CurrentBlock.NextBlock = newExecutedBlock;
            CurrentBlock = newExecutedBlock;
        }


    }
}
