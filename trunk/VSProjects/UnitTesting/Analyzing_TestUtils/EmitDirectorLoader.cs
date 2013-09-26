using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace UnitTesting.Analyzing_TestUtils
{
    class EmitDirectorLoader:LoaderBase
    {
        private readonly EmitDirector _director;
        private readonly LoaderBase _wrapped;

        internal EmitDirectorLoader(EmitDirector director, LoaderBase wrappedLoader)
        {
            _director = director;
            _wrapped = wrappedLoader;
        }

        public override GeneratorBase EntryPoint
        {
            get { return new EmitDirectorGenerator(_director); }
        }

        public override GeneratorBase StaticResolve(MethodID method)
        {
            return _wrapped.StaticResolve(method);
        }

        public override MethodID DynamicResolve(MethodID method, InstanceInfo[] dynamicArgumentInfo)
        {
            return _wrapped.DynamicResolve(method, dynamicArgumentInfo);
        }
    }
}
