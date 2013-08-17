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
    public abstract class LoaderBase<MethodID,InstanceInfo>
    {
        /// <summary>
        /// Entry point for execution start
        /// </summary>
        public abstract GeneratorBase<MethodID, InstanceInfo> EntryPoint { get; }

        public abstract VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo);

        public abstract GeneratorBase<MethodID, InstanceInfo> GetGenerator(VersionedName methodName);

        public abstract VersionedName ResolveStaticInitializer(InstanceInfo info);
    }
}
