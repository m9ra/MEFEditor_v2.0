using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;


namespace UnitTesting.TypeSystem_TestUtils
{
    class EntryPointLoader:IInstructionLoader
    {
        readonly IInstructionLoader _wrapped;
        readonly VersionedName _entryPointName;
        internal EntryPointLoader(VersionedName entryPointName,IInstructionLoader wrapped)
        {
            _entryPointName = entryPointName;
            _wrapped = wrapped;
        }

        public IInstructionGenerator<MethodID, InstanceInfo> EntryPoint
        {
            get { return _wrapped.GetGenerator(_entryPointName); }
        }

        public VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return _wrapped.ResolveCallName(method, staticArgumentInfo);
        }

        public IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName)
        {
            return _wrapped.GetGenerator(methodName);
        }

        public VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            //TODO resolve
            return new VersionedName(info.TypeName + "." + info.TypeName, 33);
        }
    }
}
