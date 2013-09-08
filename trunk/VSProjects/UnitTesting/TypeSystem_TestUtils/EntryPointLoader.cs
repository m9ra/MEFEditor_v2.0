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
        readonly VersionedName _entryPointName;
        internal EntryPointLoader(VersionedName entryPointName,LoaderBase wrapped)
        {
            _entryPointName = entryPointName;
            _wrapped = wrapped;
        }

        public override GeneratorBase EntryPoint
        {
            get { return _wrapped.GetGenerator(_entryPointName); }
        }

        public override VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return _wrapped.ResolveCallName(method, staticArgumentInfo);
        }

        public override GeneratorBase GetGenerator(VersionedName methodName)
        {
            return _wrapped.GetGenerator(methodName);
        }

        public override VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            //TODO resolve
            return new VersionedName(info.TypeName + "." + info.TypeName, 33);
        }
    }
}
