using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace UnitTesting.Analyzing_TestUtils
{
    class EmitDirectorLoader : LoaderBase
    {
        private readonly EmitDirector _director;
        private readonly LoaderBase _wrapped;

        internal readonly MethodID EntryPoint = new MethodID("DirectedEntryPoint", false);

        internal EmitDirectorLoader(EmitDirector director, LoaderBase wrappedLoader)
        {
            _director = director;
            _wrapped = wrappedLoader;
        }

        public override GeneratorBase StaticResolve(MethodID method)
        {
            if (method == EntryPoint)
            {
                return new EmitDirectorGenerator(_director);
            }

            return _wrapped.StaticResolve(method);
        }

        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _wrapped.DynamicResolve(method, dynamicArgumentInfo);
        }
    }
}
