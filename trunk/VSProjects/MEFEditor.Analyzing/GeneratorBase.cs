
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Base class for generators providing methods instruction.
    /// It also provides caching services for generated instructions, therefore
    /// after each method change new generator should be created.
    /// </summary>
    public abstract class GeneratorBase
    {
        /// <summary>
        /// Gets the emitted instructions.
        /// </summary>
        /// <value>The emitted instructions.</value>
        public InstructionBatch EmittedInstructions { get; private set; }

        /// <summary>
        /// Generate instructions through given emitter.
        /// <remarks>Throwing any exception will immediately stops analyzing.</remarks>
        /// </summary>
        /// <param name="emitter">The emitter which will be used for instruction generation.</param>
        protected abstract void generate(EmitterBase emitter);

        /// <summary>
        /// Generates instructions which will be available in <see cref="EmittedInstructions"/> property.
        /// </summary>
        /// <param name="emitter">The emitter which will be used for instruction generation.</param>
        internal void Generate(EmitterBase emitter)
        {
            if (EmittedInstructions != null)
            {
                //we cache previous instructions generation
                emitter.InsertInstructions(EmittedInstructions);
                return;
            }

            generate(emitter);
            EmittedInstructions = emitter.GetEmittedInstructions();
        }
    }
}
