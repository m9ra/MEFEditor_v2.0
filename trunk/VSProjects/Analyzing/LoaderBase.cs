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

        /// <summary>
        /// When overriden it can inject any generator for any method. Injected generator
        /// wont be binded with <see cref="MethodID"/> in methods cache.
        /// </summary>
        /// <param name="name">Name of resolved method</param>
        /// <param name="argumentValues">Arguments of resolved method</param>
        /// <returns><c>null</c> if there is no injected generator, injected generator otherwise</returns>
        public virtual GeneratorBase GetOverridingGenerator(MethodID name, Instance[] argumentValues)
        {
            //by default we dont have any overriding generator
            return null;
        }
    }
}
