using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// Common base class for all instructions.
    /// </summary>
    abstract class InstructionBase
    {
        /// <summary>
        /// Storage for instruction info.
        /// </summary>
        private InstructionInfo _info;

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public abstract void Execute(AnalyzingContext context);

        /// <summary>
        /// Info for current instruction
        /// <remarks>
        /// Info object is usually shared between multiple instructions (generated as one "block", from one line,...)
        /// </remarks>.
        /// </summary>
        /// <value>The stored <see cref="InstructionInfo"/></value>
        /// <exception cref="System.NotSupportedException">Cannot set Info twice</exception>
        /// <exception cref="System.ArgumentNullException">value</exception>
        internal InstructionInfo Info
        {
            get
            {
                return _info;
            }
            set
            {
                if (_info != null)
                    throw new NotSupportedException("Cannot set Info twice");

                if (value == null)
                    throw new ArgumentNullException("value");

                _info = value;
            }
        }        
    }
}
