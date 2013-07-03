using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments.TypeBuilding
{
    class RuntimeDefinition:RuntimeDefinition<object>
    {
    }

    class RuntimeDefinition<BaseType>
    {
        protected BaseType _base { get; private set; }
    }
}
