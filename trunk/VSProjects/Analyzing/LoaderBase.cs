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
        public abstract MethodID EntryPoint { get; }

        /// <summary>
        /// Resolve method with static argument info
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <param name="staticArgumentInfo">Static argument info, collected from argument variables</param>
        /// <returns>Resolved method name</returns>
        public abstract GeneratorBase StaticResolve(MethodID method);

        /// <summary>
        /// Resolve method with dynamic argument info
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <param name="dynamicArgumentInfo">Dynamic argument info, collected from argument instances</param>
        /// <returns>Resolved method which will be asked for generator by StaticResolve</returns>
        public abstract MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo);
    }
}
