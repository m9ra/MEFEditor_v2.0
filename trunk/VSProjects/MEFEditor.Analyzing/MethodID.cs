using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// MethodID exactly describes concrete method.
    /// <remarks>description can exists without described method existence.</remarks>
    /// </summary>
    public class MethodID
    {
        /// <summary>
        /// Name of requested method.
        /// </summary>
        public readonly string MethodString;

        /// <summary>
        /// Determine that method needs dynamic resolution for getting implementation.
        /// <remarks>This is useful for virtual methods</remarks>.
        /// </summary>
        public readonly bool NeedsDynamicResolving;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodID"/> class.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="needsDynamicResolving">if set to <c>true</c> identifier is marked with needs dynamic resolving flag.</param>
        public MethodID(string methodName, bool needsDynamicResolving)
        {
            MethodString = methodName;
            NeedsDynamicResolving = needsDynamicResolving;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as MethodID;

            if (o == null)
            {
                return false;
            }

            return o.MethodString == MethodString && o.NeedsDynamicResolving == NeedsDynamicResolving;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return MethodString.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var type = NeedsDynamicResolving ? "VirtMethod" : "Method";

            return string.Format("[{0}]{1}", type, MethodString);
        }
    }
}
