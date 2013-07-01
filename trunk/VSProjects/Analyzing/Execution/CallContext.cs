using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    public class CallContext
    {
        private Dictionary<VariableName, Instance> _variables = new Dictionary<VariableName, Instance>();
        private IInstruction[] _callInstructions;
        private uint _instructionPointer;

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed
        /// </summary>
        internal bool IsCallEnd { get { return _instructionPointer >= _callInstructions.Length; } }

        internal Instance[] ArgumentValues { get; private set; }

        internal CallContext(IInstructionLoader loader,IInstructionGenerator generator, Instance[] argumentValues)
        {
            ArgumentValues = argumentValues;
            var emitter = new CallEmitter(loader);

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
            return _variables[variable];
        }

        internal IInstruction NextInstrution()
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
    }
}
