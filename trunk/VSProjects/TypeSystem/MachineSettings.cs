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
    /// <summary>
    /// Machine settings used for analysis, configuring machine in the way that TypeSystem expect.
    /// </summary>
    public class MachineSettings : MachineSettingsBase
    {
        /// <summary>
        /// Determine that exceptions will be catched by machine or not
        /// </summary>
        private readonly bool _catchExceptions;

        /// <summary>
        /// Runtime that is used with current settings
        /// </summary>
        public readonly RuntimeAssembly Runtime = new RuntimeAssembly();

        /// </ inheritdoc>
        public override int ExecutionLimit { get { return 10000; } }

        /// </ inheritdoc>
        public override bool CatchExceptions { get { return _catchExceptions; } }

        public MachineSettings(bool catchExceptions)
        {
            _catchExceptions = catchExceptions;
        }

        /// </ inheritdoc>
        public override InstanceInfo GetNativeInfo(Type literalType)
        {
            if (literalType.IsAssignableFrom(typeof(Array<InstanceWrap>)))
            {
                return TypeDescriptor.ArrayInfo;
            }

            return TypeDescriptor.Create(literalType);
        }

        /// </ inheritdoc>
        public override bool IsTrue(Instance condition)
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

        /// </ inheritdoc>
        public override MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo)
        {
            if (IsDirect(sharedInstanceInfo))
                //direct types doesn't have static initializers 
                //TODO this could be potentionall inconsitency drawback
                return null;

            return Naming.Method(sharedInstanceInfo, Naming.ClassCtorName, false, new ParameterTypeInfo[] { });
        }

        /// </ inheritdoc>
        public override bool IsDirect(InstanceInfo typeInfo)
        {
            return Runtime.IsDirectType(typeInfo);
        }
    }
}
