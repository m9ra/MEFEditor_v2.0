using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public class MethodID
    {
        public readonly string MethodName;

        public MethodID(string methodName)
        {
            MethodName = methodName;
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

            return o.MethodName == MethodName;
        }

        public override int GetHashCode()
        {
            return MethodName.GetHashCode();
        }
    }
}
