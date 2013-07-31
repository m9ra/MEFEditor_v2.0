using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    public class CallContext<MethodID, InstanceInfo>
    {        
        private Dictionary<VariableName, Instance> _variables = new Dictionary<VariableName, Instance>();
        private IInstruction<MethodID, InstanceInfo>[] _callInstructions;
        private uint _instructionPointer;

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed
        /// </summary>
        internal bool IsCallEnd { get { return _instructionPointer >= _callInstructions.Length; } }

        internal Instance[] ArgumentValues { get; private set; }

        internal CallContext(IMachineSettings<InstanceInfo> settings,IInstructionLoader<MethodID, InstanceInfo> loader, IInstructionGenerator<MethodID, InstanceInfo> generator, Instance[] argumentValues)
        {
            ArgumentValues = argumentValues;

            var emitter = new CallEmitter<MethodID, InstanceInfo>(settings,loader);
            generator.Generate(emitter);

            _callInstructions = emitter.GetEmittedInstructions();
            _instructionPointer = 0;
        }

        internal void SetValue(VariableName targetVaraiable, Instance value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value cannot be null");
            }
            _variables[targetVaraiable] = value;
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

        internal IInstruction<MethodID, InstanceInfo> NextInstrution()
        {
            if (IsCallEnd)
            {
                return null;
            }

            return _callInstructions[_instructionPointer++];
        }

        public bool Contains(VariableName targetVariable)
        {
            return _variables.ContainsKey(targetVariable);
        }

        internal void Jump(Label target)
        {
            _instructionPointer = target.InstructionOffset;
        }
    }
}
