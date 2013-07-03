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
    public interface IInstructionLoader<MethodID,InstanceInfo>
    {
        /// <summary>
        /// Entry point for execution start
        /// </summary>
        IInstructionGenerator<MethodID, InstanceInfo> EntryPoint { get; }
        
        VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo);
        
        IInstructionGenerator<MethodID,InstanceInfo> GetGenerator(VersionedName methodName);

        VersionedName ResolveStaticInitializer(InstanceInfo info);
    }
}
