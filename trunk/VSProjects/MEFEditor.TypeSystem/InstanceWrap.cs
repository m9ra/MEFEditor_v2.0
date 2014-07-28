using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Represents wrapped instance that can be pasted as native object
    /// to native generic type definitions.
    /// </summary>
    public class InstanceWrap
    {
        /// <summary>
        /// The wrapped instance.
        /// </summary>
        public readonly Instance Wrapped;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceWrap" /> class.
        /// </summary>
        /// <param name="wrapped">The wrapped instance.</param>
        public InstanceWrap(Instance wrapped)
        {
            Wrapped = wrapped;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var direct = Wrapped as DirectInstance;

            if (direct == null)
                return Wrapped.GetHashCode();

            return direct.DirectValue.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var o = obj as InstanceWrap;
            if (o == null)
                return false;

            var direct = Wrapped as DirectInstance;
            var oDirect = o.Wrapped as DirectInstance;

            if (direct == null || oDirect == null)
                return Wrapped.Equals(o.Wrapped);

            return direct.DirectValue.Equals(oDirect.DirectValue);
        }
    }
}
