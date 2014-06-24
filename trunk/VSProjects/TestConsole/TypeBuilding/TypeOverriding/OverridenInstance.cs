using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeExperiments.Core;

namespace TypeExperiments.TypeBuilding.TypeOverriding
{
    class OverridenInstance:Instance
    {
        internal object OverridenObject;

        public OverridenInstance(InternalType type)
            : base(type)
        {
            //TODO implement
            OverridenObject = null;
        }
    }
}
