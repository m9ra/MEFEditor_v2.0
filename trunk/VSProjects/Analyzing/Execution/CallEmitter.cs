using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution.Instructions;

namespace Analyzing.Execution
{
    class CallEmitter:IEmitter
    {
        /// <summary>
        /// Instructions emitted by this emitter
        /// </summary>
        List<IInstruction> _instructions = new List<IInstruction>();
        public void AssignLiteral(string targetVar, object literal)
        {
            var target = new VariableName(targetVar);
            var literlInstance = new Instance(literal);

            _instructions.Add(new AssignLiteral(target, literlInstance));
        }

        public void Assign(string targetVar, string sourceVar)
        {
            var target = new VariableName(targetVar);
            var source = new VariableName(sourceVar);

            _instructions.Add(new Assign(target, source));
        }

        /// <summary>
        /// Get emitted program
        /// </summary>
        /// <returns>Program that has been emitted</returns>
        internal IInstruction[] GetEmittedInstructions()
        {
            return _instructions.ToArray();
        }
    }
}
