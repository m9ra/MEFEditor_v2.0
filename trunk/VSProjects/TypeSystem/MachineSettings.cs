using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem
{
    public delegate void OnInstanceCreated(Instance instance);

    public class MachineSettings:IMachineSettings
    {
        private readonly OnInstanceCreated _onInstanceCreated;

        public MachineSettings(OnInstanceCreated onInstanceCreated)
        {
            if (onInstanceCreated == null)
                throw new ArgumentNullException("onInstanceCreated");

            _onInstanceCreated = onInstanceCreated;
        }

        public InstanceInfo GetNativeInfo(Type literalType)
        {
            return new InstanceInfo(literalType.FullName);
        }

        public InstanceInfo GetSharedInstanceInfo(string typeFullname)
        {
            return new InstanceInfo(typeFullname);
        }

        public bool IsTrue(Instance condition)
        {
            return (bool)condition.DirectValue;
        }

        public MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo)
        {
            return Naming.Method(sharedInstanceInfo, "#initializer", new TypeParameterInfo[] { });
        }

        public void InstanceCreated(Instance instance)
        {
            _onInstanceCreated(instance);   
        }
    }
}
