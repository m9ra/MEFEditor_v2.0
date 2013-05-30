
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public interface IInstructionGenerator
    {
        /// <summary>
        /// Versioned name of generator - can be used for cache invalidation
        /// </summary>
        VersionedName Name { get; }
        /// <summary>
        /// Generate instructions through given emitter
        /// <remarks>Throwing any exception will immediately stops analyzing</remarks>
        /// </summary>
        /// <param name="emitter">Emitter used for instruction generating</param>
        void Generate(IEmitter emitter);
    }
}
