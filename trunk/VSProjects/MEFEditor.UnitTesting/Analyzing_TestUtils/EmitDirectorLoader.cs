using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

namespace MEFEditor.UnitTesting.Analyzing_TestUtils
{
    /// <summary>
    /// Test loader that is directed by <see cref="EmitDirector"/>.
    /// </summary>
    class EmitDirectorLoader : LoaderBase
    {
        /// <summary>
        /// The director
        /// </summary>
        private readonly EmitDirector _director;

        /// <summary>
        /// The wrapped loader.
        /// </summary>
        private readonly LoaderBase _wrapped;

        /// <summary>
        /// The entry point method identifier.
        /// </summary>
        internal readonly MethodID EntryPoint = new MethodID("DirectedEntryPoint", false);

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitDirectorLoader"/> class.
        /// </summary>
        /// <param name="director">The director that is used for entry method.</param>
        /// <param name="wrappedLoader">The wrapped loader.</param>
        internal EmitDirectorLoader(EmitDirector director, LoaderBase wrappedLoader)
        {
            _director = director;
            _wrapped = wrappedLoader;
        }

        /// <summary>
        /// Resolve method with static argument info
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <returns>Resolved method name</returns>
        public override GeneratorBase StaticResolve(MethodID method)
        {
            if (method == EntryPoint)
            {
                return new DirectedGenerator(_director);
            }

            return _wrapped.StaticResolve(method);
        }

        /// <summary>
        /// Resolve method with dynamic argument info
        /// </summary>
        /// <param name="method">Resolved method</param>
        /// <param name="dynamicArgumentInfo">Dynamic argument info, collected from argument instances</param>
        /// <returns>Resolved method which will be asked for generator by StaticResolve</returns>
        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _wrapped.DynamicResolve(method, dynamicArgumentInfo);
        }
    }
}
