using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Direct (native) method that can be interpreted by <see cref="Machine"/>.
    /// </summary>
    /// <param name="context">Context of analysis</param>
    public delegate void DirectMethod(AnalyzingContext context);
    
    /// <summary>
    /// Base class for <see cref="Machine"/> settings.
    /// </summary>
    public abstract class MachineSettingsBase
    {
        /// <summary>
        /// Event fired before interpretation by machine starts
        /// </summary>
        public event Action BeforeInterpretation;

        /// <summary>
        /// Event fired after interpretation is ended
        /// </summary>
        public event Action AfterInterpretation;

        /// <summary>
        /// Limit of instruction count that can be interpreted.
        /// </summary>
        /// <value>The execution limit.</value>
        public abstract int ExecutionLimit { get; }

        /// <summary>
        /// Limit of instance count that can be created.
        /// </summary>
        /// <value>The instance limit.</value>
        public abstract int InstanceLimit { get; }

        /// <summary>
        /// Determine that machine will catch all exceptions from interpreting
        /// and provide them as part of <see cref="AnalyzingResult" /> or not.
        /// </summary>
        /// <value><c>true</c> if exceptions should be catched by <see cref="Machine"/>; otherwise, <c>false</c>.</value>
        public abstract bool CatchExceptions { get; }

        /// <summary>
        /// Determine that instance described by given info is represented by
        /// <see cref="DirectInstance" />.
        /// </summary>
        /// <param name="info">Checked info.</param>
        /// <returns><c>true</c> if instance is represented by <see cref="DirectInstance" />, false otherwise.</returns>
        public abstract bool IsDirect(InstanceInfo info);

        /// <summary>
        /// Gets the native information.
        /// </summary>
        /// <param name="literalType">Type of the literal.</param>
        /// <returns>InstanceInfo.</returns>
        public abstract InstanceInfo GetNativeInfo(Type literalType);

        /// <summary>
        /// Determines whether the specified condition is true.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns><c>true</c> if the specified condition is true; otherwise, <c>false</c>.</returns>
        public abstract bool IsTrue(Instance condition);

        /// <summary>
        /// Gets the shared initializer for <see cref="Instance"/> with given info.
        /// </summary>
        /// <param name="sharedInstanceInfo">Instance info.</param>
        /// <returns>Initializer identifier.</returns>
        public abstract MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo);

        /// <summary>
        /// Creates the null representation that will be used by <see cref="Machine"/>.
        /// </summary>
        /// <returns>Null representation.</returns>
        public abstract object CreateNullRepresentation();

        /// <summary>
        /// Fires before interpretation handler.
        /// </summary>
        internal void FireBeforeInterpretation()
        {
            if (BeforeInterpretation != null)
                BeforeInterpretation();
        }

        /// <summary>
        /// Fires after interpretation handler.
        /// </summary>
        internal void FireAfterInterpretation()
        {
            if (AfterInterpretation != null)
                AfterInterpretation();
        }
    }
}
