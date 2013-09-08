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
    public abstract class LoaderBase
    {
        /// <summary>
        /// Entry point for execution start
        /// </summary>
        public abstract GeneratorBase EntryPoint { get; }

        public abstract VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo);

        public abstract GeneratorBase GetGenerator(VersionedName methodName);

        public abstract VersionedName ResolveStaticInitializer(InstanceInfo info);
    }
}
