using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class Instance
    {        
        internal readonly VersionedName TypeName;
        internal bool IsDirty { get; private set; }

        internal Instance(VersionedName typeName)
        {
            TypeName = typeName;
        }
    }
}
