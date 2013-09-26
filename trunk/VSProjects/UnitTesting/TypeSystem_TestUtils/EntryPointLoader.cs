using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;


namespace UnitTesting.TypeSystem_TestUtils
{
    class EntryPointLoader:LoaderBase
    {
        readonly LoaderBase _wrapped;
        readonly MethodID _entryPointName;
        internal EntryPointLoader(MethodID entryPointName,LoaderBase wrapped)
        {
            _entryPointName = entryPointName;
            _wrapped = wrapped;
        }

        public override GeneratorBase EntryPoint
        {
            get { return _wrapped.StaticResolve(_entryPointName); }
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
