using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

using TypeSystem.Runtime;

namespace TypeSystem
{
    public delegate void OnInstanceCreated(Instance instance);

    public class MachineSettings : IMachineSettings
    {
        private readonly OnInstanceCreated _onInstanceCreated;

        public readonly RuntimeAssembly Runtime;

        public MachineSettings(OnInstanceCreated onInstanceCreated)
        {
            if (onInstanceCreated == null)
                throw new ArgumentNullException("onInstanceCreated");

            Runtime = new RuntimeAssembly();

            _onInstanceCreated = onInstanceCreated;
        }

        public InstanceInfo GetNativeInfo(Type literalType)
        {
            if (literalType.IsAssignableFrom(typeof(Array<InstanceWrap>)))
            {
                return RuntimeAssembly.ArrayInfo;
            }

            return new InstanceInfo(literalType);
        }

        public InstanceInfo GetSharedInstanceInfo(string typeFullname)
        {
            return new InstanceInfo(typeFullname);
        }

        public bool IsTrue(Instance condition)
        {
            var dirVal = condition.DirectValue;
            if (dirVal is bool)
            {
                return (bool)dirVal;
            }
            else if (dirVal is int)
            {
                return (int)dirVal != 0;
            }

            return false;
        }

        public MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo)
        {
            if (IsDirect(sharedInstanceInfo))
                //direct types doesn't have static initializers 
                //TODO this could be potentionall inconsitency drawback
                return null;

            return Naming.Method(sharedInstanceInfo, Naming.ClassCtorName, false, new ParameterTypeInfo[] { });
        }

        public void InstanceCreated(Instance instance)
        {
            _onInstanceCreated(instance);
        }

        public bool IsDirect(InstanceInfo typeInfo)
        {
            return Runtime.IsDirectType(typeInfo);
        }
    }
}
