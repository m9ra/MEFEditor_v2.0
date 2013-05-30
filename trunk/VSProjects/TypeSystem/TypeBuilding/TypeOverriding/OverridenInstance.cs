using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

namespace TypeSystem.TypeBuilding.TypeOverriding
{
    class OverridenInstance:Instance
    {
        internal object OverridenObject;

        public OverridenInstance(InternalType type)
            : base(type)
        {
        }
    }
}
