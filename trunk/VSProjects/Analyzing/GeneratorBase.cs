
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public abstract class GeneratorBase
    {
        public InstructionBatch EmittedInstructions { get; private set; }

        /// <summary>
        /// Generate instructions through given emitter
        /// <remarks>Throwing any exception will immediately stops analyzing</remarks>
        /// </summary>
        /// <param name="emitter">Emitter used for instruction generating</param>
        protected abstract void generate(EmitterBase emitter);

        internal void Generate(EmitterBase emitter){
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
