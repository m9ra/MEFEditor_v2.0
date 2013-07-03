
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public interface IInstructionGenerator<MethodID,InstanceInfo>
    {
        /// <summary>
        /// Generate instructions through given emitter
        /// <remarks>Throwing any exception will immediately stops analyzing</remarks>
        /// </summary>
        /// <param name="emitter">Emitter used for instruction generating</param>
        void Generate(IEmitter<MethodID,InstanceInfo> emitter);
    }
}
