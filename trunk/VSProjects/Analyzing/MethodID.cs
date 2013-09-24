using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class MethodID
    {
        /// <summary>
        /// Name of requested method
        /// </summary>
        public readonly string MethodString;

        /// <summary>
        /// Determine that method needs dynamic resolution for versioned name
        /// <remarks>This is usefull for virtual methods</remarks>
        /// </summary>
        public readonly bool NeedsDynamicResolving;

        public MethodID(string methodName, bool needsDynamicResolving)
        {
            MethodString = methodName;
            NeedsDynamicResolving = needsDynamicResolving;
        }

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

        public override int GetHashCode()
        {
            return MethodString.GetHashCode();
        }

        public override string ToString()
        {
            var type = NeedsDynamicResolving ? "VirtMethod" : "Method";

            return string.Format("[{0}]{1}", type, MethodString);
        }
    }
}
