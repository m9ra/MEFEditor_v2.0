
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public abstract class GeneratorBase
    {
        private AnalyzingContext _lastCachedContext;

        public InstructionBatch EmittedInstructions { get; private set; }

        /// <summary>
        /// Generate instructions through given emitter
        /// <remarks>Throwing any exception will immediately stops analyzing</remarks>
        /// </summary>
        /// <param name="emitter">Emitter used for instruction generating</param>
        protected abstract void generate(EmitterBase emitter);

        internal void Generate(EmitterBase emitter){
            if (EmittedInstructions != null /*&& _lastCachedContext==emitter.Context*/)
            {
                //we cache previous instructions generation
                emitter.InsertInstructions(EmittedInstructions);
                return;
            }
          
            generate(emitter);
            _lastCachedContext = emitter.Context;
            EmittedInstructions = emitter.GetEmittedInstructions();
        }
    }
}
