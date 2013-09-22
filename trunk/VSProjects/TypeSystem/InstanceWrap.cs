using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem
{
    /// <summary>
    /// Represents wrap over instance - forward object calls to wrapped instance (or direct value)
    /// </summary>
    public sealed class InstanceWrap
    {
        internal readonly Instance Wrapped;

        internal InstanceWrap(Instance wrapped)
        {
            Wrapped = wrapped;
        }

        public override int GetHashCode()
        {
            var direct = Wrapped as DirectInstance;

            if (direct == null)
                return Wrapped.GetHashCode();

            return direct.DirectValue.GetHashCode();
        }

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
