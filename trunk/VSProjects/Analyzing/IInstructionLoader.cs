using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    /// <summary>
    /// Loader provide access to instruction generators (types, methods,..)
    /// </summary>
    public interface IInstructionLoader
    {
        /// <summary>
        /// Entry point for execution start
        /// </summary>
        IInstructionGenerator EntryPoint { get; }



        TypeDescription ResolveDescription(string typeFullname);

        VersionedName ResolveCallName(MethodDescription method);
        
        IInstructionGenerator GetGenerator(VersionedName methodName);
    }
}
