using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Machine settings used for analysis, configuring machine in the way that TypeSystem expect.
    /// </summary>
    public class MachineSettings : MachineSettingsBase
    {
        /// <summary>
        /// Determine that exceptions will be catched by machine or not.
        /// </summary>
        private readonly bool _catchExceptions;

        /// <summary>
        /// Here is stored value for codebase.
        /// </summary>
        private string _codeBase = "";

        /// <summary>
        /// Runtime that is used with current settings.
        /// </summary>
        public readonly RuntimeAssembly Runtime = new RuntimeAssembly();

        /// <summary>
        /// Determine current code base.
        /// </summary>
        /// <value>The code base full path.</value>
        public string CodeBaseFullPath
        {
            get { return _codeBase; }
            set
            {
                if (value == null)
                    value = "";

                _codeBase = value;
            }
        }

        /// <summary>
        /// Limit of instruction count that can be interpreted.
        /// </summary>
        /// <value>The execution limit.</value>
        /// <inheritdoc />
        public override int ExecutionLimit { get { return 10000; } }

        /// <summary>
        /// Limit of instance count that can be created.
        /// </summary>
        /// <value>The instance limit.</value>
        /// <inheritdoc />
        public override int InstanceLimit { get { return 1000; } }

        /// <summary>
        /// Determine that machine will catch all exceptions from interpreting
        /// and provide them as part of <see cref="AnalyzingResult" /> or not.
        /// </summary>
        /// <value><c>true</c> if exceptions should be catched by <see cref="Machine" />; otherwise, <c>false</c>.</value>
        /// <inheritdoc />
        public override bool CatchExceptions { get { return _catchExceptions; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MachineSettings"/> class.
        /// </summary>
        /// <param name="catchExceptions">if set to <c>true</c> [catch exceptions].</param>
        public MachineSettings(bool catchExceptions)
        {
            _catchExceptions = catchExceptions;
        }

        /// <summary>
        /// Gets the native information.
        /// </summary>
        /// <param name="literalType">Type of the literal.</param>
        /// <returns>InstanceInfo.</returns>
        /// <inheritdoc />
        public override InstanceInfo GetNativeInfo(Type literalType)
        {
            if (literalType.IsAssignableFrom(typeof(Array<InstanceWrap>)))
            {
                return TypeDescriptor.ArrayInfo;
            }

            return TypeDescriptor.Create(literalType);
        }

        /// <summary>
        /// Determines whether the specified condition is true.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns><c>true</c> if the specified condition is true; otherwise, <c>false</c>.</returns>
        /// <inheritdoc />
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

        /// <summary>
        /// Gets the shared initializer for <see cref="Instance" /> with given info.
        /// </summary>
        /// <param name="sharedInstanceInfo">Instance info.</param>
        /// <returns>Initializer identifier.</returns>
        /// <inheritdoc />
        public override MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo)
        {
            if (IsDirect(sharedInstanceInfo))
                //direct types doesn't have static initializers 
                return null;

            return Naming.Method(sharedInstanceInfo, Naming.ClassCtorName, false, new ParameterTypeInfo[] { });
        }

        /// <summary>
        /// Determines whether the specified type information is direct.
        /// </summary>
        /// <param name="typeInfo">The type information.</param>
        /// <returns><c>true</c> if the specified type information is direct; otherwise, <c>false</c>.</returns>
        /// <inheritdoc />
        public override bool IsDirect(InstanceInfo typeInfo)
        {
            return Runtime.IsDirectType(typeInfo);
        }

        /// <summary>
        /// Creates the null representation that will be used by <see cref="Machine" />.
        /// </summary>
        /// <returns>Null representation.</returns>
        /// <inheritdoc />
        public override object CreateNullRepresentation()
        {
            return new Null();
        }
    }
}
