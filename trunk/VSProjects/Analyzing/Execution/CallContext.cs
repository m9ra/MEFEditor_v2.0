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
        public readonly InstructionBatch<MethodID, InstanceInfo> Program;
        private uint _instructionPointer;
        private LinkedList<CallContext<MethodID, InstanceInfo>> _childContexts = new LinkedList<CallContext<MethodID, InstanceInfo>>();

        /// <summary>
        /// Determine that call doesn't have next instructions to proceed
        /// </summary>
        internal bool IsCallEnd { get { return _instructionPointer >= Program.Instructions.Length; } }

        internal Instance[] ArgumentValues { get; private set; }

        public IEnumerable<CallContext<MethodID, InstanceInfo>> ChildContexts { get { return _childContexts; } }

        public readonly VersionedName Name;

       

  


        internal CallContext(IMachineSettings<InstanceInfo> settings, LoaderBase<MethodID, InstanceInfo> loader,VersionedName name, GeneratorBase<MethodID, InstanceInfo> generator, Instance[] argumentValues)
        {
            ArgumentValues = argumentValues;
            Name = name;

            var emitter = new CallEmitter<MethodID, InstanceInfo>(settings, loader);
            generator.Generate(emitter);

            Program = emitter.GetEmittedInstructions();
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

        internal InstructionBase<MethodID, InstanceInfo> NextInstrution()
        {
            if (IsCallEnd)
            {
                return null;
            }

            return Program.Instructions[_instructionPointer++];
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
            _childContexts.AddLast(call);
        }
    }
}
